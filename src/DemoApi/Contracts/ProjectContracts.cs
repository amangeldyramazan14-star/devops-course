using System.ComponentModel.DataAnnotations;
using DemoApi.Domain;

namespace DemoApi.Contracts;

public sealed record CreateProjectRequest(
    [Required, StringLength(100)] string Name);

public sealed record ProjectResponse(int Id, string Name, DateTimeOffset CreatedAt)
{
    public static ProjectResponse From(Project p) => new(p.Id, p.Name, p.CreatedAt);
}
