using TaskManager.Api.Models;

namespace TaskManager.Api.Services;

public interface ITaskExportService
{
    Task<IReadOnlyList<TaskItem>> ExportAsync(CancellationToken cancellationToken);
}
