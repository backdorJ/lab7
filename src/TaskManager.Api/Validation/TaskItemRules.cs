using Microsoft.AspNetCore.Mvc.ModelBinding;
using TaskManager.Api.Models;

namespace TaskManager.Api.Validation;

public static class TaskItemRules
{
    public static void Normalize(TaskItem item, ModelStateDictionary modelState)
    {
        item.Title = (item.Title ?? string.Empty).Trim();
        item.Description = (item.Description ?? string.Empty).Trim();

        if (item.Title.Length == 0)
        {
            modelState.AddModelError(nameof(TaskItem.Title), "Укажите название задачи");
        }
    }
}
