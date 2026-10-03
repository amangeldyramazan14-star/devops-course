using DemoApi.Data;
using DemoApi.Domain;
using DemoApi.Errors;

namespace DemoApi.Services;

public sealed class TaskService(ITaskRepository tasks, IProjectRepository projects, ILogger<TaskService> log)
{
    public Task<IReadOnlyList<TaskItem>> ListAsync(int? projectId, TaskItemStatus? status, CancellationToken ct) =>
        tasks.ListAsync(projectId, status, ct);

    public async Task<TaskItem> GetAsync(int id, CancellationToken ct) =>
        await tasks.GetAsync(id, ct) ?? throw NotFound(id);

    public async Task<TaskItem> CreateAsync(int projectId, string title, string? description, DateOnly? dueDate, CancellationToken ct)
    {
        _ = await projects.GetAsync(projectId, ct)
            ?? throw new NotFoundException($"Проект {projectId} не найден");

        var task = await tasks.CreateAsync(projectId, title.Trim(), description, dueDate, ct);
        log.LogInformation("Создана задача {TaskId} в проекте {ProjectId}", task.Id, task.ProjectId);
        return task;
    }

    public async Task<TaskItem> UpdateAsync(int id, string title, string? description, DateOnly? dueDate, CancellationToken ct) =>
        await tasks.UpdateAsync(id, title.Trim(), description, dueDate, ct) ?? throw NotFound(id);

    public async Task<TaskItem> ChangeStatusAsync(int id, TaskItemStatus status, CancellationToken ct)
    {
        var task = await GetAsync(id, ct);
        if (!StatusRules.CanTransition(task.Status, status))
        {
            throw new ConflictException($"Нельзя перевести задачу {id} из {task.Status} в {status}");
        }

        var updated = await tasks.SetStatusAsync(id, status, ct) ?? throw NotFound(id);
        log.LogInformation("Задача {TaskId}: {OldStatus} -> {NewStatus}", id, task.Status, status);
        return updated;
    }

    public async Task DeleteAsync(int id, CancellationToken ct)
    {
        if (!await tasks.DeleteAsync(id, ct))
        {
            throw NotFound(id);
        }
    }

    private static NotFoundException NotFound(int id) => new($"Задача {id} не найдена");
}
