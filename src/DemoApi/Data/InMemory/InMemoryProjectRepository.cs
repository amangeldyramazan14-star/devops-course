using DemoApi.Domain;
using DemoApi.Errors;

namespace DemoApi.Data.InMemory;

public sealed class InMemoryProjectRepository(InMemoryStore store) : IProjectRepository
{
    public Task<IReadOnlyList<Project>> ListAsync(CancellationToken ct)
    {
        lock (store.Sync)
        {
            IReadOnlyList<Project> result = store.Projects.OrderBy(p => p.Id).ToList();
            return Task.FromResult(result);
        }
    }

    public Task<Project?> GetAsync(int id, CancellationToken ct)
    {
        lock (store.Sync)
        {
            return Task.FromResult(store.Projects.FirstOrDefault(p => p.Id == id));
        }
    }

    public Task<Project> CreateAsync(string name, CancellationToken ct)
    {
        lock (store.Sync)
        {
            EnsureNameIsFree(name, exceptId: null);
            var project = new Project(store.NextProjectId++, name, DateTimeOffset.UtcNow);
            store.Projects.Add(project);
            return Task.FromResult(project);
        }
    }

    public Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        lock (store.Sync)
        {
            store.Tasks.RemoveAll(t => t.ProjectId == id);   // как ON DELETE CASCADE в БД
            return Task.FromResult(store.Projects.RemoveAll(p => p.Id == id) > 0);
        }
    }

    // Имя проекта уникально — как ограничение UNIQUE в таблице projects
    private void EnsureNameIsFree(string name, int? exceptId)
    {
        if (store.Projects.Any(p => p.Name == name && p.Id != exceptId))
        {
            throw new ConflictException($"Проект с именем «{name}» уже существует");
        }
    }
}
