using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using DemoApi.Data;
using DemoApi.Data.InMemory;
using DemoApi.Errors;
using DemoApi.Services;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Npgsql;
using Prometheus;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// В контейнере — структурные JSON-логи (удобно для сбора и поиска)
if (!builder.Environment.IsDevelopment())
{
    builder.Logging.ClearProviders();
    builder.Logging.AddJsonConsole(o => o.JsonWriterOptions = new JsonWriterOptions
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,   // кириллица в логах без \uXXXX
    });
}

// Хранилище: InMemory — данные в памяти процесса (темы 1–2), Postgres — PostgreSQL (с темы 3)
var usePostgres = string.Equals(builder.Configuration["Storage"], "Postgres", StringComparison.OrdinalIgnoreCase);

// live  — процесс жив (без проверок зависимостей)
// ready — сервис готов принимать трафик (для PostgreSQL — БД доступна)
var healthChecks = builder.Services.AddHealthChecks();

if (usePostgres)
{
    var connectionString = builder.Configuration.GetConnectionString("Db")
        ?? throw new InvalidOperationException("Не задана строка подключения ConnectionStrings:Db");

    builder.Services.AddNpgsqlDataSource(connectionString);
    builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
    builder.Services.AddScoped<ITaskRepository, TaskRepository>();
    healthChecks.AddNpgSql(connectionString, name: "db", tags: ["ready"]);
}
else
{
    builder.Services.AddSingleton<InMemoryStore>();
    builder.Services.AddScoped<IProjectRepository, InMemoryProjectRepository>();
    builder.Services.AddScoped<ITaskRepository, InMemoryTaskRepository>();
}

// Бизнес-логика
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<TaskService>();

// Web API
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddOpenApi();

var app = builder.Build();

app.Logger.LogInformation("Хранилище: {Storage}", usePostgres ? "PostgreSQL" : "в памяти");

if (usePostgres)
{
    await DatabaseInitializer.EnsureSchemaAsync(app.Services.GetRequiredService<NpgsqlDataSource>());
}

app.UseHttpMetrics();        // снаружи обработчика ошибок: в метрики попадает итоговый код ответа (500)
app.UseExceptionHandler();   // исключения -> ProblemDetails (404, 409, 500)

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();                // /openapi/v1.json — описание API
    app.MapScalarApiReference();     // /scalar — страница, где можно отправлять запросы из браузера
}

app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = c => c.Tags.Contains("ready") });
app.MapMetrics();            // GET /metrics
app.MapControllers();

app.Run();

public partial class Program { }
