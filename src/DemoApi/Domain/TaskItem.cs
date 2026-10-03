namespace DemoApi.Domain;

public sealed record TaskItem(
    int Id,
    int ProjectId,
    string Title,
    string? Description,
    TaskItemStatus Status,
    DateOnly? DueDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
