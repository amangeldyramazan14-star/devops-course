using DemoApi.Contracts;
using DemoApi.Domain;
using DemoApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace DemoApi.Controllers;

[ApiController]
[Route("api/tasks")]
[Produces("application/json")]
public sealed class TasksController(TaskService tasks) : ControllerBase
{
    // GET /api/tasks?projectId=1&status=InProgress
    [HttpGet]
    public async Task<IEnumerable<TaskResponse>> List(
        [FromQuery] int? projectId, [FromQuery] TaskItemStatus? status, CancellationToken ct) =>
        (await tasks.ListAsync(projectId, status, ct)).Select(TaskResponse.From);

    [HttpGet("{id:int}")]
    [ProducesResponseType<TaskResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<TaskResponse> Get(int id, CancellationToken ct) =>
        TaskResponse.From(await tasks.GetAsync(id, ct));

    [HttpPost]
    [ProducesResponseType<TaskResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskResponse>> Create(CreateTaskRequest request, CancellationToken ct)
    {
        var task = await tasks.CreateAsync(request.ProjectId, request.Title, request.Description, request.DueDate, ct);
        return CreatedAtAction(nameof(Get), new { id = task.Id }, TaskResponse.From(task));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<TaskResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<TaskResponse> Update(int id, UpdateTaskRequest request, CancellationToken ct) =>
        TaskResponse.From(await tasks.UpdateAsync(id, request.Title, request.Description, request.DueDate, ct));

    [HttpPatch("{id:int}/status")]
    [ProducesResponseType<TaskResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<TaskResponse> ChangeStatus(int id, ChangeStatusRequest request, CancellationToken ct) =>
        TaskResponse.From(await tasks.ChangeStatusAsync(id, request.Status!.Value, ct));

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await tasks.DeleteAsync(id, ct);
        return NoContent();
    }
}
