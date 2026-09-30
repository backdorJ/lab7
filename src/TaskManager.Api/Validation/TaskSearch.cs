using TaskManager.Api.Models;

namespace TaskManager.Api.Validation;

public static class TaskSearch
{
    public static IEnumerable<TaskItem> Filter(IEnumerable<TaskItem> tasks, string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return tasks;
        }

        var term = query.Trim();
        return tasks.Where(task =>
            task.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
            task.Description.Contains(term, StringComparison.OrdinalIgnoreCase));
    }
}
