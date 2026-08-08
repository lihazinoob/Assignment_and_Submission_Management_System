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

    public async Task<Submission> GradeSubmissionAsync(
        Guid submissionId,
        decimal marksObtained,
        string? feedback,
        Guid gradedBy,
        CancellationToken cancellationToken = default)
    {
        var submission = await _context.Submissions
            .Include(s => s.Assignment)
            .FirstOrDefaultAsync(s => s.Id == submissionId, cancellationToken);

        if (submission is null)
        {
            throw new BusinessRuleException($"Submission '{submissionId}' was not found.");
        }

        if (marksObtained > submission.Assignment.MaxMarks)
        {
            throw new BusinessRuleException(
                $"marks_obtained ({marksObtained}) cannot exceed assignment max_marks ({submission.Assignment.MaxMarks}).");
        }

        submission.MarksObtained = marksObtained;
        submission.Feedback = feedback;
        submission.GradedBy = gradedBy;
        submission.GradedAt = DateTime.UtcNow;
        submission.Status = SubmissionStatus.Graded;

        await _context.SaveChangesAsync(cancellationToken);

        return submission;
    }
}
