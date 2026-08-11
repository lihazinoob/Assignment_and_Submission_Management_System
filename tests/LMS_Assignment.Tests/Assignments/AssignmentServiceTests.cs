using LMS_Assignment.Application.Assignments;
using LMS_Assignment.Application.Common.Exceptions;
using LMS_Assignment.Domain.Entities;
using LMS_Assignment.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LMS_Assignment.Tests.Assignments;

public class AssignmentServiceTests
{
    private static AssignmentService CreateSut(TestApplicationDbContext context) => new(context);

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

    private static async Task<TeacherSubjectAssignment> SeedTeacherSubjectAssignmentAsync(TestApplicationDbContext context, User teacher)
    {
        var @class = new Class { Id = Guid.NewGuid(), Name = "Grade 10 - Section A", AcademicYear = "2026" };
        var subject = new Subject { Id = Guid.NewGuid(), Name = "Mathematics", Code = "MATH101" };
        var classSubject = new ClassSubject { Id = Guid.NewGuid(), ClassId = @class.Id, SubjectId = subject.Id };
        var tsa = new TeacherSubjectAssignment { Id = Guid.NewGuid(), TeacherId = teacher.Id, ClassSubjectId = classSubject.Id };

        context.Classes.Add(@class);
        context.Subjects.Add(subject);
        context.ClassSubjects.Add(classSubject);
        context.TeacherSubjectAssignments.Add(tsa);
        await context.SaveChangesAsync();

        return tsa;
    }

    private static async Task<Assignment> SeedAssignmentAsync(
        TestApplicationDbContext context,
        TeacherSubjectAssignment tsa,
        AssignmentStatus status = AssignmentStatus.Draft)
    {
        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            TeacherSubjectAssignmentId = tsa.Id,
            Title = "Homework 1",
            Deadline = DateTime.UtcNow.AddDays(7),
            MaxMarks = 100,
            Status = status
        };

        context.Assignments.Add(assignment);
        await context.SaveChangesAsync();
        return assignment;
    }

    [Fact]
    public async Task CreateAsync_ByOwningTeacher_CreatesDraftAssignment()
    {
        var context = TestApplicationDbContext.CreateNew();
        var sut = CreateSut(context);
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var tsa = await SeedTeacherSubjectAssignmentAsync(context, teacher);

        var result = await sut.CreateAsync(tsa.Id, "Homework 1", "Do exercises 1-5", DateTime.UtcNow.AddDays(7), 100, true, teacher.Id);

        Assert.Equal(AssignmentStatus.Draft, result.Status);
        Assert.Equal("Homework 1", result.Title);
    }

    [Fact]
    public async Task CreateAsync_ByNonOwningTeacher_ThrowsForbiddenAccessException()
    {
        var context = TestApplicationDbContext.CreateNew();
        var sut = CreateSut(context);
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var otherTeacher = await SeedUserAsync(context, UserRole.Teacher);
        var tsa = await SeedTeacherSubjectAssignmentAsync(context, teacher);

        await Assert.ThrowsAsync<ForbiddenAccessException>(
            () => sut.CreateAsync(tsa.Id, "Homework 1", null, DateTime.UtcNow.AddDays(7), 100, true, otherTeacher.Id));
    }

    [Fact]
    public async Task CreateAsync_WithNonPositiveMaxMarks_ThrowsBusinessRuleException()
    {
        var context = TestApplicationDbContext.CreateNew();
        var sut = CreateSut(context);
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var tsa = await SeedTeacherSubjectAssignmentAsync(context, teacher);

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => sut.CreateAsync(tsa.Id, "Homework 1", null, DateTime.UtcNow.AddDays(7), 0, true, teacher.Id));
    }

    [Fact]
    public async Task CreateAsync_WithDeactivatedClass_ThrowsBusinessRuleException()
    {
        var context = TestApplicationDbContext.CreateNew();
        var sut = CreateSut(context);
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var tsa = await SeedTeacherSubjectAssignmentAsync(context, teacher);
        tsa.ClassSubject.Class.IsActive = false;
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => sut.CreateAsync(tsa.Id, "Homework 1", null, DateTime.UtcNow.AddDays(7), 100, true, teacher.Id));
    }

    [Fact]
    public async Task CreateAsync_WithDeactivatedClassSubject_ThrowsBusinessRuleException()
    {
        var context = TestApplicationDbContext.CreateNew();
        var sut = CreateSut(context);
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var tsa = await SeedTeacherSubjectAssignmentAsync(context, teacher);
        tsa.ClassSubject.IsActive = false;
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => sut.CreateAsync(tsa.Id, "Homework 1", null, DateTime.UtcNow.AddDays(7), 100, true, teacher.Id));
    }

    [Fact]
    public async Task UpdateAsync_OnPublishedAssignment_ThrowsBusinessRuleException()
    {
        var context = TestApplicationDbContext.CreateNew();
        var sut = CreateSut(context);
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var tsa = await SeedTeacherSubjectAssignmentAsync(context, teacher);
        var assignment = await SeedAssignmentAsync(context, tsa, AssignmentStatus.Published);

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => sut.UpdateAsync(assignment.Id, "New Title", null, DateTime.UtcNow.AddDays(10), 50, true, teacher.Id));
    }

    [Fact]
    public async Task UpdateAsync_WithDeactivatedClass_ThrowsBusinessRuleException()
    {
        var context = TestApplicationDbContext.CreateNew();
        var sut = CreateSut(context);
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var tsa = await SeedTeacherSubjectAssignmentAsync(context, teacher);
        var assignment = await SeedAssignmentAsync(context, tsa);
        tsa.ClassSubject.Class.IsActive = false;
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => sut.UpdateAsync(assignment.Id, "New Title", null, DateTime.UtcNow.AddDays(10), 50, true, teacher.Id));
    }

    [Fact]
    public async Task UpdateAsync_WithDeactivatedClassSubject_ThrowsBusinessRuleException()
    {
        var context = TestApplicationDbContext.CreateNew();
        var sut = CreateSut(context);
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var tsa = await SeedTeacherSubjectAssignmentAsync(context, teacher);
        var assignment = await SeedAssignmentAsync(context, tsa);
        tsa.ClassSubject.IsActive = false;
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => sut.UpdateAsync(assignment.Id, "New Title", null, DateTime.UtcNow.AddDays(10), 50, true, teacher.Id));
    }

    [Fact]
    public async Task UpdateAsync_ByNonOwningTeacher_ThrowsForbiddenAccessException()
    {
        var context = TestApplicationDbContext.CreateNew();
        var sut = CreateSut(context);
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var otherTeacher = await SeedUserAsync(context, UserRole.Teacher);
        var tsa = await SeedTeacherSubjectAssignmentAsync(context, teacher);
        var assignment = await SeedAssignmentAsync(context, tsa);

        await Assert.ThrowsAsync<ForbiddenAccessException>(
            () => sut.UpdateAsync(assignment.Id, "New Title", null, DateTime.UtcNow.AddDays(10), 50, true, otherTeacher.Id));
    }

    [Fact]
    public async Task PublishAsync_OnDraftAssignmentByOwner_SetsPublished()
    {
        var context = TestApplicationDbContext.CreateNew();
        var sut = CreateSut(context);
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var tsa = await SeedTeacherSubjectAssignmentAsync(context, teacher);
        var assignment = await SeedAssignmentAsync(context, tsa);

        var result = await sut.PublishAsync(assignment.Id, teacher.Id);

        Assert.Equal(AssignmentStatus.Published, result.Status);
    }

    [Fact]
    public async Task PublishAsync_OnAlreadyPublishedAssignment_ThrowsBusinessRuleException()
    {
        var context = TestApplicationDbContext.CreateNew();
        var sut = CreateSut(context);
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var tsa = await SeedTeacherSubjectAssignmentAsync(context, teacher);
        var assignment = await SeedAssignmentAsync(context, tsa, AssignmentStatus.Published);

        await Assert.ThrowsAsync<BusinessRuleException>(() => sut.PublishAsync(assignment.Id, teacher.Id));
    }

    [Fact]
    public async Task PublishAsync_WithDeactivatedClass_ThrowsBusinessRuleException()
    {
        var context = TestApplicationDbContext.CreateNew();
        var sut = CreateSut(context);
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var tsa = await SeedTeacherSubjectAssignmentAsync(context, teacher);
        var assignment = await SeedAssignmentAsync(context, tsa);
        tsa.ClassSubject.Class.IsActive = false;
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<BusinessRuleException>(() => sut.PublishAsync(assignment.Id, teacher.Id));
    }

    [Fact]
    public async Task PublishAsync_WithDeactivatedClassSubject_ThrowsBusinessRuleException()
    {
        var context = TestApplicationDbContext.CreateNew();
        var sut = CreateSut(context);
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var tsa = await SeedTeacherSubjectAssignmentAsync(context, teacher);
        var assignment = await SeedAssignmentAsync(context, tsa);
        tsa.ClassSubject.IsActive = false;
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<BusinessRuleException>(() => sut.PublishAsync(assignment.Id, teacher.Id));
    }

    [Fact]
    public async Task DeleteAsync_WithExistingSubmissions_ThrowsBusinessRuleException()
    {
        var context = TestApplicationDbContext.CreateNew();
        var sut = CreateSut(context);
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var student = await SeedUserAsync(context, UserRole.Student);
        var tsa = await SeedTeacherSubjectAssignmentAsync(context, teacher);
        var assignment = await SeedAssignmentAsync(context, tsa, AssignmentStatus.Published);

        context.Submissions.Add(new Submission { Id = Guid.NewGuid(), AssignmentId = assignment.Id, StudentId = student.Id });
        await context.SaveChangesAsync();

        await Assert.ThrowsAsync<BusinessRuleException>(() => sut.DeleteAsync(assignment.Id, teacher.Id));
    }

    [Fact]
    public async Task DeleteAsync_WithNoSubmissions_SoftDeletesAssignment()
    {
        var context = TestApplicationDbContext.CreateNew();
        var sut = CreateSut(context);
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var tsa = await SeedTeacherSubjectAssignmentAsync(context, teacher);
        var assignment = await SeedAssignmentAsync(context, tsa);

        await sut.DeleteAsync(assignment.Id, teacher.Id);

        var stored = await context.Assignments.SingleAsync(a => a.Id == assignment.Id);
        Assert.True(stored.IsDeleted);
    }

    [Fact]
    public async Task DeleteAsync_WithDeactivatedClass_StillSucceeds()
    {
        // Unlike Create/Update/Publish, deleting a Teacher's own draft assignment is cleanup,
        // not new activity, so it stays allowed even once the class has been deactivated.
        var context = TestApplicationDbContext.CreateNew();
        var sut = CreateSut(context);
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var tsa = await SeedTeacherSubjectAssignmentAsync(context, teacher);
        var assignment = await SeedAssignmentAsync(context, tsa);
        tsa.ClassSubject.Class.IsActive = false;
        await context.SaveChangesAsync();

        await sut.DeleteAsync(assignment.Id, teacher.Id);

        var stored = await context.Assignments.SingleAsync(a => a.Id == assignment.Id);
        Assert.True(stored.IsDeleted);
    }

    [Fact]
    public async Task GetForCurrentUserAsync_AsStudent_OnlyReturnsPublishedAssignmentsForEnrolledClass()
    {
        var context = TestApplicationDbContext.CreateNew();
        var sut = CreateSut(context);
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var student = await SeedUserAsync(context, UserRole.Student);
        var tsa = await SeedTeacherSubjectAssignmentAsync(context, teacher);
        var draftAssignment = await SeedAssignmentAsync(context, tsa, AssignmentStatus.Draft);
        var publishedAssignment = await SeedAssignmentAsync(context, tsa, AssignmentStatus.Published);

        context.StudentEnrollments.Add(new StudentEnrollment
        {
            Id = Guid.NewGuid(),
            StudentId = student.Id,
            ClassId = tsa.ClassSubject.ClassId,
            Status = EnrollmentStatus.Active
        });
        await context.SaveChangesAsync();

        var result = await sut.GetForCurrentUserAsync(student.Id, UserRole.Student);

        var resultId = Assert.Single(result).Id;
        Assert.Equal(publishedAssignment.Id, resultId);
        Assert.DoesNotContain(result, a => a.Id == draftAssignment.Id);
    }

    [Fact]
    public async Task GetByIdAsync_AsStudentNotEnrolled_ThrowsForbiddenAccessException()
    {
        var context = TestApplicationDbContext.CreateNew();
        var sut = CreateSut(context);
        var teacher = await SeedUserAsync(context, UserRole.Teacher);
        var student = await SeedUserAsync(context, UserRole.Student);
        var tsa = await SeedTeacherSubjectAssignmentAsync(context, teacher);
        var assignment = await SeedAssignmentAsync(context, tsa, AssignmentStatus.Published);

        await Assert.ThrowsAsync<ForbiddenAccessException>(
            () => sut.GetByIdAsync(assignment.Id, student.Id, UserRole.Student));
    }
}
