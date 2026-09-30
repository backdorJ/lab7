namespace TaskManager.Api.Options;

public class ExportOptions
{
    public const string SectionName = "Export";

    public int IntervalSeconds { get; set; } = 30;

    public string FilePath { get; set; } = "tasks_export.json";
}
