using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.Models;
using TaskManager.Api.Services;
using TaskManager.Api.Validation;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/tasks")]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ITaskExportService _exportService;

    public TasksController(AppDbContext db, ITaskExportService exportService)
    {
        _db = db;
        _exportService = exportService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TaskItem>>> GetAll(CancellationToken cancellationToken)
    {
        var tasks = await _db.Tasks
            .AsNoTracking()
            .OrderBy(task => task.Id)
            .ToListAsync(cancellationToken);

        return Ok(tasks);
    }

    [HttpGet("search")]
    public async Task<ActionResult<PagedResult<TaskItem>>> Search(
        [FromQuery] string? q,
        [FromQuery] int page = 1,
        [FromQuery] int size = 10,
        CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        size = size < 1 ? 10 : Math.Min(size, 100);

        var tasks = await _db.Tasks.AsNoTracking().ToListAsync(cancellationToken);
        var filtered = TaskSearch.Filter(tasks, q)
            .OrderBy(task => task.Id)
            .ToList();

        var total = filtered.Count;
        var items = filtered
            .Skip((page - 1) * size)
            .Take(size)
            .ToList();

        return Ok(new PagedResult<TaskItem>
        {
            Items = items,
            Page = page,
            Size = size,
            Total = total
        });
    }

    [HttpGet("export")]
    public async Task<ActionResult<IReadOnlyList<TaskItem>>> Export(CancellationToken cancellationToken)
    {
        var tasks = await _exportService.ExportAsync(cancellationToken);
        return Ok(tasks);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskItem>> GetById(int id, CancellationToken cancellationToken)
    {
        var task = await _db.Tasks
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (task is null)
        {
            return NotFound(new { error = $"Задача {id} не найдена" });
        }

        return Ok(task);
    }

    [HttpPost]
    public async Task<ActionResult<TaskItem>> Create(TaskItem item, CancellationToken cancellationToken)
    {
        TaskItemRules.Normalize(item, ModelState);
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        item.Id = 0;
        _db.Tasks.Add(item);
        await _db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, TaskItem item, CancellationToken cancellationToken)
    {
        TaskItemRules.Normalize(item, ModelState);
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var existing = await _db.Tasks.FirstOrDefaultAsync(task => task.Id == id, cancellationToken);
        if (existing is null)
        {
            return NotFound(new { error = $"Задача {id} не найдена" });
        }

        existing.Title = item.Title;
        existing.Description = item.Description;
        existing.DueDate = item.DueDate;
        existing.IsDone = item.IsDone;
        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var existing = await _db.Tasks.FirstOrDefaultAsync(task => task.Id == id, cancellationToken);
        if (existing is null)
        {
            return NotFound(new { error = $"Задача {id} не найдена" });
        }

        _db.Tasks.Remove(existing);
        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
