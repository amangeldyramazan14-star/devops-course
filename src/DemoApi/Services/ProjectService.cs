using DemoApi.Data;
using DemoApi.Domain;
using DemoApi.Errors;

namespace DemoApi.Services;

public sealed class ProjectService(IProjectRepository projects, ILogger<ProjectService> log)
{
    public Task<IReadOnlyList<Project>> ListAsync(CancellationToken ct) => projects.ListAsync(ct);

    public async Task<Project> GetAsync(int id, CancellationToken ct) =>
        await projects.GetAsync(id, ct) ?? throw NotFound(id);

    public async Task<Project> CreateAsync(string name, CancellationToken ct)
    {
        var project = await projects.CreateAsync(name.Trim(), ct);
        log.LogInformation("Создан проект {ProjectId} «{ProjectName}»", project.Id, project.Name);
        return project;
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        if (!await projects.DeleteAsync(id, ct))
        {
            throw NotFound(id);
        }
        log.LogInformation("Удалён проект {ProjectId}", id);
    }

        public async Task<ProjectStats> GetStatsAsync(int id, CancellationToken ct)
    {
        await GetAsync(id, ct);
        return await projects.GetStatsAsync(id, ct);
    }

    private static NotFoundException NotFound(int id) => new($"Проект {id} не найден");
}
