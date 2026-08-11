using LMS_Assignment.Application.Common.Exceptions;
using LMS_Assignment.Application.Common.Interfaces;
using LMS_Assignment.Domain.Entities;
using LMS_Assignment.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LMS_Assignment.Application.Submissions;

public class SubmissionService : ISubmissionService
{
    private readonly IApplicationDbContext _context;

    public SubmissionService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Submission> SubmitAsync(
        Guid assignmentId,
        string? answerText,
        Guid studentId,
        CancellationToken cancellationToken = default)
    {
        var assignment = await _context.Assignments
            .Include(a => a.TeacherSubjectAssignment).ThenInclude(t => t.ClassSubject).ThenInclude(cs => cs.Class)
            .FirstOrDefaultAsync(a => a.Id == assignmentId && !a.IsDeleted, cancellationToken);

        if (assignment is null)
        {
            throw new BusinessRuleException($"No assignment found with id '{assignmentId}'.");
        }

        if (assignment.Status != AssignmentStatus.Published)
        {
            throw new BusinessRuleException("You can only submit to published assignments.");
        }

        if (!assignment.TeacherSubjectAssignment.ClassSubject.Class.IsActive)
        {
            throw new BusinessRuleException("This class has been deactivated. No new activity is allowed.");
        }

        if (!assignment.TeacherSubjectAssignment.ClassSubject.IsActive)
        {
            throw new BusinessRuleException("This class-subject link has been deactivated. No new activity is allowed.");
        }

        var classId = assignment.TeacherSubjectAssignment.ClassSubject.ClassId;
        var activeStatus = EnrollmentStatus.Active;
        var isEnrolled = await _context.StudentEnrollments.AnyAsync(
            e => e.StudentId == studentId && e.ClassId == classId && e.Status == activeStatus,
            cancellationToken);

        if (!isEnrolled)
        {
            throw new ForbiddenAccessException("You are not enrolled in the class this assignment belongs to.");
        }

        var alreadySubmitted = await _context.Submissions.AnyAsync(
            s => s.AssignmentId == assignmentId && s.StudentId == studentId,
            cancellationToken);

        if (alreadySubmitted)
        {
            throw new BusinessRuleException("You have already submitted this assignment. Use update instead.");
        }

        var student = await _context.Users.FirstOrDefaultAsync(u => u.Id == studentId, cancellationToken);
        if (student is null)
        {
            throw new BusinessRuleException($"No user found with id '{studentId}'.");
        }

        var now = DateTime.UtcNow;
        var status = now > assignment.Deadline ? SubmissionStatus.Late : SubmissionStatus.Submitted;

        var submission = new Submission
        {
            Id = Guid.NewGuid(),
            AssignmentId = assignmentId,
            StudentId = studentId,
            AnswerText = answerText,
            SubmittedAt = now,
            UpdatedAt = now,
            Status = status,
            Assignment = assignment,
            Student = student
        };

        _context.Submissions.Add(submission);
        await _context.SaveChangesAsync(cancellationToken);

        return submission;
    }

    public async Task<Submission> UpdateAsync(
        Guid submissionId,
        string? answerText,
        Guid studentId,
        CancellationToken cancellationToken = default)
    {
        var submission = await LoadOwnedSubmissionAsync(submissionId, studentId, cancellationToken);

        if (!submission.Assignment.TeacherSubjectAssignment.ClassSubject.Class.IsActive)
        {
            throw new BusinessRuleException("This class has been deactivated. No new activity is allowed.");
        }

        if (!submission.Assignment.TeacherSubjectAssignment.ClassSubject.IsActive)
        {
            throw new BusinessRuleException("This class-subject link has been deactivated. No new activity is allowed.");
        }

        if (!submission.Assignment.AllowResubmission)
        {
            throw new BusinessRuleException("This assignment does not allow resubmission.");
        }

        if (submission.Status == SubmissionStatus.Graded)
        {
            throw new BusinessRuleException("You cannot update a submission that has already been graded.");
        }

        if (DateTime.UtcNow > submission.Assignment.Deadline)
        {
            throw new BusinessRuleException("The deadline for this assignment has passed.");
        }

        submission.AnswerText = answerText;
        submission.SubmittedAt = DateTime.UtcNow;
        submission.Status = SubmissionStatus.Submitted;

        await _context.SaveChangesAsync(cancellationToken);

        return submission;
    }

    public async Task<Submission> GradeSubmissionAsync(
        Guid submissionId,
        decimal marksObtained,
        string? feedback,
        Guid teacherId,
        CancellationToken cancellationToken = default)
    {
        var submission = await _context.Submissions
            .Include(s => s.Student)
            .Include(s => s.Assignment).ThenInclude(a => a.TeacherSubjectAssignment)
            .FirstOrDefaultAsync(s => s.Id == submissionId, cancellationToken);

        if (submission is null)
        {
            throw new BusinessRuleException($"Submission '{submissionId}' was not found.");
        }

        if (submission.Assignment.TeacherSubjectAssignment.TeacherId != teacherId)
        {
            throw new ForbiddenAccessException("You can only grade submissions for your own assignments.");
        }

        if (marksObtained < 0)
        {
            throw new BusinessRuleException("marks_obtained cannot be negative.");
        }

        if (marksObtained > submission.Assignment.MaxMarks)
        {
            throw new BusinessRuleException(
                $"marks_obtained ({marksObtained}) cannot exceed assignment max_marks ({submission.Assignment.MaxMarks}).");
        }

        submission.MarksObtained = marksObtained;
        submission.Feedback = feedback;
        submission.GradedBy = teacherId;
        submission.GradedAt = DateTime.UtcNow;
        submission.Status = SubmissionStatus.Graded;

        await _context.SaveChangesAsync(cancellationToken);

        return submission;
    }

    public async Task<List<Submission>> GetForCurrentUserAsync(
        Guid userId,
        UserRole role,
        Guid? assignmentId,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Submissions
            .Include(s => s.Student)
            .Include(s => s.Assignment)
            .AsQueryable();

        switch (role)
        {
            case UserRole.Admin:
                if (assignmentId.HasValue)
                {
                    query = query.Where(s => s.AssignmentId == assignmentId.Value);
                }

                return await query.OrderByDescending(s => s.SubmittedAt).ToListAsync(cancellationToken);

            case UserRole.Teacher:
                if (!assignmentId.HasValue)
                {
                    throw new BusinessRuleException("assignmentId is required to view submissions as a teacher.");
                }

                var assignment = await _context.Assignments
                    .Include(a => a.TeacherSubjectAssignment)
                    .FirstOrDefaultAsync(a => a.Id == assignmentId.Value, cancellationToken);

                if (assignment is null)
                {
                    throw new BusinessRuleException($"No assignment found with id '{assignmentId.Value}'.");
                }

                if (assignment.TeacherSubjectAssignment.TeacherId != userId)
                {
                    throw new ForbiddenAccessException("You can only view submissions for your own assignments.");
                }

                return await query
                    .Where(s => s.AssignmentId == assignmentId.Value)
                    .OrderByDescending(s => s.SubmittedAt)
                    .ToListAsync(cancellationToken);

            case UserRole.Student:
                query = query.Where(s => s.StudentId == userId);
                if (assignmentId.HasValue)
                {
                    query = query.Where(s => s.AssignmentId == assignmentId.Value);
                }

                return await query.OrderByDescending(s => s.SubmittedAt).ToListAsync(cancellationToken);

            default:
                return new List<Submission>();
        }
    }

    public async Task<Submission> GetByIdAsync(Guid submissionId, Guid userId, UserRole role, CancellationToken cancellationToken = default)
    {
        var submission = await _context.Submissions
            .Include(s => s.Student)
            .Include(s => s.Assignment).ThenInclude(a => a.TeacherSubjectAssignment)
            .FirstOrDefaultAsync(s => s.Id == submissionId, cancellationToken);

        if (submission is null)
        {
            throw new BusinessRuleException($"Submission '{submissionId}' was not found.");
        }

        switch (role)
        {
            case UserRole.Admin:
                return submission;

            case UserRole.Teacher:
                if (submission.Assignment.TeacherSubjectAssignment.TeacherId != userId)
                {
                    throw new ForbiddenAccessException("You can only view submissions for your own assignments.");
                }

                return submission;

            case UserRole.Student:
                if (submission.StudentId != userId)
                {
                    throw new ForbiddenAccessException("You can only view your own submissions.");
                }

                return submission;

            default:
                throw new ForbiddenAccessException("You are not allowed to view this submission.");
        }
    }

    private async Task<Submission> LoadOwnedSubmissionAsync(Guid submissionId, Guid studentId, CancellationToken cancellationToken)
    {
        var submission = await _context.Submissions
            .Include(s => s.Student)
            .Include(s => s.Assignment).ThenInclude(a => a.TeacherSubjectAssignment).ThenInclude(t => t.ClassSubject).ThenInclude(cs => cs.Class)
            .FirstOrDefaultAsync(s => s.Id == submissionId, cancellationToken);

        if (submission is null)
        {
            throw new BusinessRuleException($"Submission '{submissionId}' was not found.");
        }

        if (submission.StudentId != studentId)
        {
            throw new ForbiddenAccessException("You can only modify your own submissions.");
        }

        return submission;
    }
}
