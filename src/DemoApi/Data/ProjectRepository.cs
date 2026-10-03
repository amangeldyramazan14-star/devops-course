using DemoApi.Domain;
using DemoApi.Errors;
using Npgsql;
using NpgsqlTypes;

namespace DemoApi.Data;

public interface IProjectRepository
{
    Task<IReadOnlyList<Project>> ListAsync(CancellationToken ct);
    Task<Project?> GetAsync(int id, CancellationToken ct);
    Task<Project> CreateAsync(string name, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
}

public sealed class ProjectRepository(NpgsqlDataSource db) : IProjectRepository
{
    private const string Columns = "id, name, created_at";

    public async Task<IReadOnlyList<Project>> ListAsync(CancellationToken ct)
    {
        await using var cmd = db.CreateCommand($"SELECT {Columns} FROM projects ORDER BY id");
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        var result = new List<Project>();
        while (await reader.ReadAsync(ct))
        {
            result.Add(Map(reader));
        }
        return result;
    }

    public async Task<Project?> GetAsync(int id, CancellationToken ct)
    {
        await using var cmd = db.CreateCommand($"SELECT {Columns} FROM projects WHERE id = $1");
        cmd.Parameters.Add(Sql.Param(id, NpgsqlDbType.Integer));
        await using var reader = await cmd.ExecuteReaderAsync(ct);
        return await reader.ReadAsync(ct) ? Map(reader) : null;
    }

    public async Task<Project> CreateAsync(string name, CancellationToken ct)
    {
        await using var cmd = db.CreateCommand($"INSERT INTO projects (name) VALUES ($1) RETURNING {Columns}");
        cmd.Parameters.Add(Sql.Param(name, NpgsqlDbType.Text));
        try
        {
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            await reader.ReadAsync(ct);
            return Map(reader);
        }
        catch (PostgresException e) when (e.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException($"Проект с именем «{name}» уже существует");
        }
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        await using var cmd = db.CreateCommand("DELETE FROM projects WHERE id = $1");
        cmd.Parameters.Add(Sql.Param(id, NpgsqlDbType.Integer));
        return await cmd.ExecuteNonQueryAsync(ct) > 0;
    }

    private static Project Map(NpgsqlDataReader r) =>
        new(r.GetInt32(0), r.GetString(1), r.GetFieldValue<DateTimeOffset>(2));
}
