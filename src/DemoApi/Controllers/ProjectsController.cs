using DemoApi.Contracts;
using DemoApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace DemoApi.Controllers;

[ApiController]
[Route("api/projects")]
[Produces("application/json")]
public sealed class ProjectsController(ProjectService projects) : ControllerBase
{
    [HttpGet]
    public async Task<IEnumerable<ProjectResponse>> List(CancellationToken ct) =>
        (await projects.ListAsync(ct)).Select(ProjectResponse.From);

    [HttpGet("{id:int}")]
    [ProducesResponseType<ProjectResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ProjectResponse> Get(int id, CancellationToken ct) =>
        ProjectResponse.From(await projects.GetAsync(id, ct));

    [HttpPost]
    [ProducesResponseType<ProjectResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProjectResponse>> Create(CreateProjectRequest request, CancellationToken ct)
    {
        var project = await projects.CreateAsync(request.Name, ct);
        return CreatedAtAction(nameof(Get), new { id = project.Id }, ProjectResponse.From(project));
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        await projects.DeleteAsync(id, ct);
        return NoContent();
    }
}
