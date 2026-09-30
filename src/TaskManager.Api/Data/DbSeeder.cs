using TaskManager.Api.Models;

namespace TaskManager.Api.Data;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.Tasks.Any())
        {
            return;
        }

        db.Tasks.AddRange(
            new TaskItem
            {
                Title = "Сдать лабораторную 7",
                Description = "Сервер, WinForms-клиент и фоновый экспорт",
                DueDate = DateTime.Today.AddDays(3),
                IsDone = false
            },
            new TaskItem
            {
                Title = "Проверить поиск API",
                Description = "Параметры q, page и size",
                DueDate = DateTime.Today.AddDays(1),
                IsDone = false
            },
            new TaskItem
            {
                Title = "Написать README",
                Description = "Инструкция запуска bat-файлов",
                DueDate = DateTime.Today.AddDays(2),
                IsDone = true
            },
            new TaskItem
            {
                Title = "Открыть веб-интерфейс",
                Description = "Просмотр и редактирование задач",
                DueDate = DateTime.Today,
                IsDone = false
            });

        db.SaveChanges();
    }
}
