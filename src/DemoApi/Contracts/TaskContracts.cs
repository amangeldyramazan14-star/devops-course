using System.ComponentModel.DataAnnotations;
using DemoApi.Domain;

namespace DemoApi.Contracts;

public sealed record CreateTaskRequest(
    [Range(1, int.MaxValue)] int ProjectId,
    [Required, StringLength(200)] string Title,
    [StringLength(2000)] string? Description,
    DateOnly? DueDate);

public sealed record UpdateTaskRequest(
    [Required, StringLength(200)] string Title,
    [StringLength(2000)] string? Description,
    DateOnly? DueDate);

public sealed record ChangeStatusRequest(
    [Required] TaskItemStatus? Status);

public sealed record TaskResponse(
    int Id,
    int ProjectId,
    string Title,
    string? Description,
    TaskItemStatus Status,
    DateOnly? DueDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public static TaskResponse From(TaskItem t) =>
        new(t.Id, t.ProjectId, t.Title, t.Description, t.Status, t.DueDate, t.CreatedAt, t.UpdatedAt);
}
