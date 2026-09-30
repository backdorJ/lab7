using Microsoft.Extensions.Options;
using TaskManager.Api.Options;

namespace TaskManager.Api.Services;

public class TaskExportBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TaskExportBackgroundService> _logger;
    private readonly int _intervalSeconds;

    public TaskExportBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<ExportOptions> options,
        ILogger<TaskExportBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _intervalSeconds = Math.Max(5, options.Value.IntervalSeconds);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Фоновый экспорт запущен. Интервал: {IntervalSeconds} с",
            _intervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var exporter = scope.ServiceProvider.GetRequiredService<ITaskExportService>();
                var tasks = await exporter.ExportAsync(stoppingToken);
                _logger.LogInformation("Экспортировано задач: {Count}", tasks.Count);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Не удалось экспортировать задачи");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(_intervalSeconds), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Фоновый экспорт завершён");
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Остановка фонового экспорта");
        await base.StopAsync(cancellationToken);
    }
}
