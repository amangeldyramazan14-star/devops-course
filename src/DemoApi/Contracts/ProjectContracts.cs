using System.ComponentModel.DataAnnotations;
using DemoApi.Domain;

namespace DemoApi.Contracts;

public sealed record CreateProjectRequest(
    [Required, StringLength(100)] string Name);

public sealed record ProjectResponse(int Id, string Name, DateTimeOffset CreatedAt)
{
    public static ProjectResponse From(Project p) => new(p.Id, p.Name, p.CreatedAt);
}

public sealed record ProjectStatsResponse(
    int ProjectId,
    int Total,
    int Todo,
    int InProgress,
    int Done,
    int Overdue)
{
    public static ProjectStatsResponse From(ProjectStats s) =>
        new(s.ProjectId, s.Total, s.Todo, s.InProgress, s.Done, s.Overdue);
}
