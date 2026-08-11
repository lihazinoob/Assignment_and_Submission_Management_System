using LMS_Assignment.Application.Common.Exceptions;
using LMS_Assignment.Application.Submissions;
using LMS_Assignment.Domain.Entities;
using LMS_Assignment.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LMS_Assignment.Tests.Submissions;

public class SubmissionServiceTests
{
    private static SubmissionService CreateSut(TestApplicationDbContext context) => new(context);

    private static async Task<User> SeedUserAsync(TestApplicationDbContext context, UserRole role)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            FullName = $"{role} User",
            Email = $"{Guid.NewGuid()}@lms.demo",
            PasswordHash = "irrelevant",
            Role = role,
            IsActive = true
        };

        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    private static async Task<Assignment> SeedPublishedAssignmentAsync(
        TestApplicationDbContext context,
        User teacher,
        User student,
        bool allowResubmission = true,
        DateTime? deadline = null)
    {
        var @class = new Class { Id = Guid.NewGuid(), Name = "Grade 10 - Section A", AcademicYear = "2026" };
        var subject = new Subject { Id = Guid.NewGuid(), Name = "Mathematics", Code = "MATH101" };
        var classSubject = new ClassSubject { Id = Guid.NewGuid(), ClassId = @class.Id, SubjectId = subject.Id };
        var tsa = new TeacherSubjectAssignment { Id = Guid.NewGuid(), TeacherId = teacher.Id, ClassSubjectId = classSubject.Id };
        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            TeacherSubjectAssignmentId = tsa.Id,
            Title = "Homework 1",
            Deadline = deadline ?? DateTime.UtcNow.AddDays(7),
            MaxMarks = 100,
            AllowResubmission = allowResubmission,
            Status = AssignmentStatus.Published
        };
        var enrollment = new StudentEnrollment
        {
            Id = Guid.NewGuid(),
            StudentId = student.Id,
            ClassId = @class.Id,
            Status = EnrollmentStatus.Active
        };

        context.Classes.Add(@class);
        context.Subjects.Add(subject);
        context.ClassSubjects.Add(classSubject);
        context.TeacherSubjectAssignments.Add(tsa);
        context.Assignments.Add(assignment);
        context.StudentEnrollments.Add(enrollment);
        await context.SaveChangesAsync();

        return assignment;
    }

    [Fact]
    public async Task SubmitAsync_ByEnrolledStudent_CreatesSubmission()
    {
        var context = TestApplicationDbContext.CreateNew();
        var sut = CreateSut(context);
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var student = await SeedUserAsync(context, UserRole.Student);
        var assignment = await SeedPublishedAssignmentAsync(context, teacher, student);

        var result = await sut.SubmitAsync(assignment.Id, "My answer", student.Id);

        Assert.Equal(SubmissionStatus.Submitted, result.Status);
        Assert.Equal(student.Id, result.StudentId);
    }

    [Fact]
    public async Task SubmitAsync_PastDeadline_MarksAsLate()
    {
        var context = TestApplicationDbContext.CreateNew();
        var sut = CreateSut(context);
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var student = await SeedUserAsync(context, UserRole.Student);
        var assignment = await SeedPublishedAssignmentAsync(context, teacher, student, deadline: DateTime.UtcNow.AddDays(-1));

        var result = await sut.SubmitAsync(assignment.Id, "Late answer", student.Id);

        Assert.Equal(SubmissionStatus.Late, result.Status);
    }

    [Fact]
    public async Task SubmitAsync_ByUnenrolledStudent_ThrowsForbiddenAccessException()
    {
        var context = TestApplicationDbContext.CreateNew();
        var sut = CreateSut(context);
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var enrolledStudent = await SeedUserAsync(context, UserRole.Student);
        var outsideStudent = await SeedUserAsync(context, UserRole.Student);
        var assignment = await SeedPublishedAssignmentAsync(context, teacher, enrolledStudent);

        await Assert.ThrowsAsync<ForbiddenAccessException>(
            () => sut.SubmitAsync(assignment.Id, "Answer", outsideStudent.Id));
    }

    [Fact]
    public async Task SubmitAsync_OnDraftAssignment_ThrowsBusinessRuleException()
    {
        var context = TestApplicationDbContext.CreateNew();
        var sut = CreateSut(context);
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var student = await SeedUserAsync(context, UserRole.Student);
        var assignment = await SeedPublishedAssignmentAsync(context, teacher, student);
        assignment.Status = AssignmentStatus.Draft;
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<BusinessRuleException>(() => sut.SubmitAsync(assignment.Id, "Answer", student.Id));
    }

    [Fact]
    public async Task SubmitAsync_WhenAlreadySubmitted_ThrowsBusinessRuleException()
    {
        var context = TestApplicationDbContext.CreateNew();
        var sut = CreateSut(context);
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var student = await SeedUserAsync(context, UserRole.Student);
        var assignment = await SeedPublishedAssignmentAsync(context, teacher, student);

        await sut.SubmitAsync(assignment.Id, "First answer", student.Id);

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => sut.SubmitAsync(assignment.Id, "Second answer", student.Id));
    }

    [Fact]
    public async Task SubmitAsync_WithDeactivatedClass_ThrowsBusinessRuleException()
    {
        var context = TestApplicationDbContext.CreateNew();
        var sut = CreateSut(context);
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var student = await SeedUserAsync(context, UserRole.Student);
        var assignment = await SeedPublishedAssignmentAsync(context, teacher, student);
        assignment.TeacherSubjectAssignment.ClassSubject.Class.IsActive = false;
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => sut.SubmitAsync(assignment.Id, "Answer", student.Id));
    }

    [Fact]
    public async Task SubmitAsync_WithDeactivatedClassSubject_ThrowsBusinessRuleException()
    {
        var context = TestApplicationDbContext.CreateNew();
        var sut = CreateSut(context);
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var student = await SeedUserAsync(context, UserRole.Student);
        var assignment = await SeedPublishedAssignmentAsync(context, teacher, student);
        assignment.TeacherSubjectAssignment.ClassSubject.IsActive = false;
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => sut.SubmitAsync(assignment.Id, "Answer", student.Id));
    }

    [Fact]
    public async Task UpdateAsync_ByOwningStudentBeforeDeadline_UpdatesAnswer()
    {
        var context = TestApplicationDbContext.CreateNew();
        var sut = CreateSut(context);
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var student = await SeedUserAsync(context, UserRole.Student);
        var assignment = await SeedPublishedAssignmentAsync(context, teacher, student);
        var submission = await sut.SubmitAsync(assignment.Id, "First answer", student.Id);

        var result = await sut.UpdateAsync(submission.Id, "Revised answer", student.Id);

        Assert.Equal("Revised answer", result.AnswerText);
    }

    [Fact]
    public async Task UpdateAsync_ByNonOwningStudent_ThrowsForbiddenAccessException()
    {
        var context = TestApplicationDbContext.CreateNew();
        var sut = CreateSut(context);
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var student = await SeedUserAsync(context, UserRole.Student);
        var otherStudent = await SeedUserAsync(context, UserRole.Student);
        var assignment = await SeedPublishedAssignmentAsync(context, teacher, student);
        var submission = await sut.SubmitAsync(assignment.Id, "First answer", student.Id);

        await Assert.ThrowsAsync<ForbiddenAccessException>(
            () => sut.UpdateAsync(submission.Id, "Hijack", otherStudent.Id));
    }

    [Fact]
    public async Task UpdateAsync_WhenResubmissionNotAllowed_ThrowsBusinessRuleException()
    {
        var context = TestApplicationDbContext.CreateNew();
        var sut = CreateSut(context);
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var student = await SeedUserAsync(context, UserRole.Student);
        var assignment = await SeedPublishedAssignmentAsync(context, teacher, student, allowResubmission: false);
        var submission = await sut.SubmitAsync(assignment.Id, "First answer", student.Id);

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => sut.UpdateAsync(submission.Id, "Revised", student.Id));
    }

    [Fact]
    public async Task UpdateAsync_WithDeactivatedClass_ThrowsBusinessRuleException()
    {
        var context = TestApplicationDbContext.CreateNew();
        var sut = CreateSut(context);
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var student = await SeedUserAsync(context, UserRole.Student);
        var assignment = await SeedPublishedAssignmentAsync(context, teacher, student);
        var submission = await sut.SubmitAsync(assignment.Id, "First answer", student.Id);
        assignment.TeacherSubjectAssignment.ClassSubject.Class.IsActive = false;
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => sut.UpdateAsync(submission.Id, "Revised", student.Id));
    }

    [Fact]
    public async Task UpdateAsync_WithDeactivatedClassSubject_ThrowsBusinessRuleException()
    {
        var context = TestApplicationDbContext.CreateNew();
        var sut = CreateSut(context);
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var student = await SeedUserAsync(context, UserRole.Student);
        var assignment = await SeedPublishedAssignmentAsync(context, teacher, student);
        var submission = await sut.SubmitAsync(assignment.Id, "First answer", student.Id);
        assignment.TeacherSubjectAssignment.ClassSubject.IsActive = false;
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => sut.UpdateAsync(submission.Id, "Revised", student.Id));
    }

    [Fact]
    public async Task UpdateAsync_AfterDeadline_ThrowsBusinessRuleException()
    {
        var context = TestApplicationDbContext.CreateNew();
        var sut = CreateSut(context);
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var student = await SeedUserAsync(context, UserRole.Student);
        var assignment = await SeedPublishedAssignmentAsync(context, teacher, student, deadline: DateTime.UtcNow.AddMinutes(1));
        var submission = await sut.SubmitAsync(assignment.Id, "First answer", student.Id);

        assignment.Deadline = DateTime.UtcNow.AddMinutes(-1);
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => sut.UpdateAsync(submission.Id, "Too late", student.Id));
    }

    [Fact]
    public async Task GradeSubmissionAsync_ByOwningTeacher_SetsGradedStatus()
    {
        var context = TestApplicationDbContext.CreateNew();
        var sut = CreateSut(context);
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var student = await SeedUserAsync(context, UserRole.Student);
        var assignment = await SeedPublishedAssignmentAsync(context, teacher, student);
        var submission = await sut.SubmitAsync(assignment.Id, "Answer", student.Id);

        var result = await sut.GradeSubmissionAsync(submission.Id, 90, "Good job", teacher.Id);

        Assert.Equal(SubmissionStatus.Graded, result.Status);
        Assert.Equal(90, result.MarksObtained);
        Assert.Equal(teacher.Id, result.GradedBy);
    }

    [Fact]
    public async Task GradeSubmissionAsync_WithDeactivatedClass_StillSucceeds()
    {
        // Grading an existing submission is finishing work that already exists, not new
        // activity, so it stays allowed even once the class has been deactivated.
        var context = TestApplicationDbContext.CreateNew();
        var sut = CreateSut(context);
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var student = await SeedUserAsync(context, UserRole.Student);
        var assignment = await SeedPublishedAssignmentAsync(context, teacher, student);
        var submission = await sut.SubmitAsync(assignment.Id, "Answer", student.Id);
        assignment.TeacherSubjectAssignment.ClassSubject.Class.IsActive = false;
        await context.SaveChangesAsync();

        var result = await sut.GradeSubmissionAsync(submission.Id, 90, "Good job", teacher.Id);

        Assert.Equal(SubmissionStatus.Graded, result.Status);
    }

    [Fact]
    public async Task GradeSubmissionAsync_WithDeactivatedClassSubject_StillSucceeds()
    {
        var context = TestApplicationDbContext.CreateNew();
        var sut = CreateSut(context);
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var student = await SeedUserAsync(context, UserRole.Student);
        var assignment = await SeedPublishedAssignmentAsync(context, teacher, student);
        var submission = await sut.SubmitAsync(assignment.Id, "Answer", student.Id);
        assignment.TeacherSubjectAssignment.ClassSubject.IsActive = false;
        await context.SaveChangesAsync();

        var result = await sut.GradeSubmissionAsync(submission.Id, 90, "Good job", teacher.Id);

        Assert.Equal(SubmissionStatus.Graded, result.Status);
    }

    [Fact]
    public async Task GradeSubmissionAsync_ByNonOwningTeacher_ThrowsForbiddenAccessException()
    {
        var context = TestApplicationDbContext.CreateNew();
        var sut = CreateSut(context);
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var otherTeacher = await SeedUserAsync(context, UserRole.Teacher);
        var student = await SeedUserAsync(context, UserRole.Student);
        var assignment = await SeedPublishedAssignmentAsync(context, teacher, student);
        var submission = await sut.SubmitAsync(assignment.Id, "Answer", student.Id);

        await Assert.ThrowsAsync<ForbiddenAccessException>(
            () => sut.GradeSubmissionAsync(submission.Id, 90, null, otherTeacher.Id));
    }

    [Fact]
    public async Task GradeSubmissionAsync_ExceedingMaxMarks_ThrowsBusinessRuleException()
    {
        var context = TestApplicationDbContext.CreateNew();
        var sut = CreateSut(context);
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var student = await SeedUserAsync(context, UserRole.Student);
        var assignment = await SeedPublishedAssignmentAsync(context, teacher, student);
        var submission = await sut.SubmitAsync(assignment.Id, "Answer", student.Id);

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => sut.GradeSubmissionAsync(submission.Id, 150, null, teacher.Id));
    }

    [Fact]
    public async Task GetForCurrentUserAsync_AsTeacherWithoutAssignmentId_ThrowsBusinessRuleException()
    {
        var context = TestApplicationDbContext.CreateNew();
        var sut = CreateSut(context);
        var teacher = await SeedUserAsync(context, UserRole.Teacher);

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => sut.GetForCurrentUserAsync(teacher.Id, UserRole.Teacher, null));
    }

    [Fact]
    public async Task GetForCurrentUserAsync_AsStudent_OnlyReturnsOwnSubmissions()
    {
        var context = TestApplicationDbContext.CreateNew();
        var sut = CreateSut(context);
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var student = await SeedUserAsync(context, UserRole.Student);
        var otherStudent = await SeedUserAsync(context, UserRole.Student);
        var assignment = await SeedPublishedAssignmentAsync(context, teacher, student);

        context.StudentEnrollments.Add(new StudentEnrollment
        {
            Id = Guid.NewGuid(),
            StudentId = otherStudent.Id,
            ClassId = assignment.TeacherSubjectAssignment.ClassSubject.ClassId,
            Status = EnrollmentStatus.Active
        });
        await context.SaveChangesAsync();

        await sut.SubmitAsync(assignment.Id, "Mine", student.Id);
        await sut.SubmitAsync(assignment.Id, "Theirs", otherStudent.Id);

        var result = await sut.GetForCurrentUserAsync(student.Id, UserRole.Student, null);

        var single = Assert.Single(result);
        Assert.Equal(student.Id, single.StudentId);
    }
}
