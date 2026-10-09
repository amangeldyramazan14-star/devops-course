namespace DemoApi.Domain;

public sealed record Project(int Id, string Name, DateTimeOffset CreatedAt);

public sealed record ProjectStats(
    int ProjectId,
    int Total,
    int Todo,
    int InProgress,
    int Done,
    int Overdue);
