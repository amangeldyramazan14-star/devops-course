using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using DemoApi.Contracts;


namespace DemoApi.Tests.Integration;

[Collection(ApiCollection.Name)]
public class ProjectsApiTests(WebApplicationFactory<Program> factory)
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Create_ReturnsCreatedWithLocation()
    {
        var response = await _client.PostAsJsonAsync("/api/projects", new { name = TestApi.Unique("project") });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task Create_DuplicateName_ReturnsConflict()
    {
        var name = TestApi.Unique("duplicate");
        (await _client.PostAsJsonAsync("/api/projects", new { name })).EnsureSuccessStatusCode();

        var response = await _client.PostAsJsonAsync("/api/projects", new { name });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_EmptyName_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/projects", new { name = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Get_Unknown_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/projects/{int.MaxValue}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetStats_ReturnsProjectStatistics()
    {
        var project = await _client.CreateProjectAsync();

        var yesterday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));

        var overdueResponse = await _client.PostAsJsonAsync(
            "/api/tasks",
            new
            {
                projectId = project.Id,
                title = "Просроченная задача",
                dueDate = yesterday
            });
        overdueResponse.EnsureSuccessStatusCode();

        var task2 = await _client.CreateTaskAsync(project.Id, "Задача в работе");
        await _client.CreateTaskAsync(project.Id, "Обычная задача");

        var statusResponse = await _client.ChangeStatusAsync(
            task2.Id,
            DemoApi.Domain.TaskItemStatus.InProgress);
        statusResponse.EnsureSuccessStatusCode();

        var response = await _client.GetAsync($"/api/projects/{project.Id}/stats");
        response.EnsureSuccessStatusCode();

        var stats = await response.Content.ReadFromJsonAsync<ProjectStatsResponse>(TestApi.Json);

        Assert.NotNull(stats);
        Assert.Equal(project.Id, stats.ProjectId);
        Assert.Equal(3, stats.Total);
        Assert.Equal(2, stats.Todo);
        Assert.Equal(1, stats.InProgress);
        Assert.Equal(0, stats.Done);
        Assert.Equal(1, stats.Overdue);
    }

    [Fact]
    public async Task GetStats_UnknownProject_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/projects/{int.MaxValue}/stats");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
