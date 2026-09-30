using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TaskManager.Api.Data;
using TaskManager.Api.Models;
using TaskManager.Api.Options;

namespace TaskManager.Api.Services;

public class TaskExportService : ITaskExportService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly AppDbContext _db;
    private readonly ExportOptions _options;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<TaskExportService> _logger;

    public TaskExportService(
        AppDbContext db,
        IOptions<ExportOptions> options,
        IHostEnvironment environment,
        ILogger<TaskExportService> logger)
    {
        _db = db;
        _options = options.Value;
        _environment = environment;
        _logger = logger;
    }

    public async Task<IReadOnlyList<TaskItem>> ExportAsync(CancellationToken cancellationToken)
    {
        var tasks = await _db.Tasks
            .AsNoTracking()
            .OrderBy(task => task.Id)
            .ToListAsync(cancellationToken);

        var path = ResolvePath(_options.FilePath);
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(tasks, JsonOptions);
        var temporaryPath = path + ".tmp";
        await File.WriteAllTextAsync(temporaryPath, json, cancellationToken);
        File.Move(temporaryPath, path, overwrite: true);

        _logger.LogInformation("Файл экспорта обновлён: {FilePath}", path);
        return tasks;
    }

    private string ResolvePath(string configuredPath)
    {
        if (Path.IsPathRooted(configuredPath))
        {
            return configuredPath;
        }

        return Path.Combine(_environment.ContentRootPath, configuredPath);
    }
}
