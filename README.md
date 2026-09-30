# Лабораторная работа 7 — TaskManager

Система из ASP.NET Core Web API и Windows Forms-клиента. Сервер хранит задачи в SQLite, отдаёт CRUD и поиск, показывает Razor-страницы и по расписанию записывает `tasks_export.json`.

## Состав

```
lab7/
  start-server.bat          запуск API и веб-интерфейса
  start-client.bat          запуск WinForms-клиента
  src/TaskManager.Api       сервер
  src/TaskManager.Client    Windows Forms
```

Сервер рассчитан на .NET 8 и запускается на Windows, Linux и macOS. Клиент — проект `net8.0-windows`: его нужно собирать и запускать в Windows (или в виртуальной машине).

## Запуск сервера

Дважды щёлкните `start-server.bat` либо выполните в каталоге решения:

```bat
start-server.bat
```

На macOS и Linux:

```bash
dotnet run --project src/TaskManager.Api --launch-profile http
```

После старта:

- веб-интерфейс: http://localhost:5080
- Swagger: http://localhost:5080/swagger
- база: `src/TaskManager.Api/tasks.db` (создаётся при первом запуске)
- журнал фонового экспорта: `src/TaskManager.Api/logs/taskmanager.log`
- файл экспорта: `src/TaskManager.Api/tasks_export.json`

Остановка сервера останавливает и `BackgroundService`: в журнале появляется строка «Остановка фонового экспорта», затем «Фоновый экспорт завершён».

## Запуск клиента

Сначала должен работать сервер. В Windows:

```bat
start-client.bat
```

Адрес API берётся из `src/TaskManager.Client/App.config`, ключ `ServerUrl` (по умолчанию `http://localhost:5080`). То же значение показано в поле «URL сервера» на форме: его можно изменить и сохранить кнопкой «Обновить» или «Добавить». Кнопка «Добавить» открывает диалог и отправляет `POST /api/tasks`. Ошибки сети и ответы 4xx/5xx показываются в окне сообщения.

## Веб-интерфейс

Страницы на том же хосте, что и API:

| Адрес | Действие |
| --- | --- |
| `/` | список и поиск |
| `/Home/Create` | создание, ошибки валидации остаются на форме |
| `/Home/Edit/{id}` | изменение |
| POST `/Home/Delete/{id}` | удаление |

Пустое название, слишком длинный текст и некорректный срок не сохраняются: форма показывает текст ошибки.

## HTTP API

| Метод | Адрес | Назначение |
| --- | --- | --- |
| GET | `/api/tasks` | все задачи |
| GET | `/api/tasks/{id}` | одна задача |
| POST | `/api/tasks` | создание |
| PUT | `/api/tasks/{id}` | обновление |
| DELETE | `/api/tasks/{id}` | удаление |
| GET | `/api/tasks/search?q=&page=1&size=10` | поиск по названию и описанию, страница с 1, размер 1–100 |
| GET | `/api/tasks/export` | JSON-массив и запись `tasks_export.json` |

Тело запроса и ответа — JSON. Невалидная задача возвращает `400` с `ValidationProblem`. Несуществующий `id` возвращает `404`.

Пример создания:

```bash
curl -X POST http://localhost:5080/api/tasks \
  -H "Content-Type: application/json" \
  -d "{\"title\":\"Проверить экспорт\",\"description\":\"Файл tasks_export.json\",\"dueDate\":\"2026-10-01\",\"isDone\":false}"
```

Поиск:

```bash
curl "http://localhost:5080/api/tasks/search?q=экспорт&page=1&size=5"
```

## Фоновый экспорт

`TaskExportBackgroundService` зарегистрирован через DI. Интервал и путь задаются в `src/TaskManager.Api/appsettings.json`:

```json
"Export": {
  "IntervalSeconds": 30,
  "FilePath": "tasks_export.json"
}
```

Минимальный интервал — 5 секунд. Каждая итерация пишет файл и строку «Экспортировано задач: N» в консоль и в `logs/taskmanager.log`. Цикл смотрит `CancellationToken` остановки приложения и выходит без ошибки, когда хост завершается.

## Что закрывает задания

1. `TasksController` — CRUD. Клиент — `DataGridView`, кнопка «Добавить», `HttpClient`, async/await, URL в `App.config`. Запуск — bat-файлы и этот README.
2. Razor-страницы `Home` с серверной валидацией. `GET /api/tasks/export` возвращает массив и записывает файл. Ошибки создания и обновления приходят кодом и текстом.
3. `BackgroundService` с настраиваемым интервалом, поиск с пагинацией, остановка службы вместе с приложением.
