using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Api.Data;
using TaskManager.Api.Models;
using TaskManager.Api.Validation;

namespace TaskManager.Api.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _db;

    public HomeController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? q)
    {
        var tasks = await _db.Tasks.AsNoTracking().ToListAsync();
        if (!string.IsNullOrWhiteSpace(q))
        {
            ViewData["Query"] = q.Trim();
        }

        var list = TaskSearch.Filter(tasks, q)
            .OrderBy(task => task.IsDone)
            .ThenBy(task => task.DueDate)
            .ThenBy(task => task.Id)
            .ToList();

        return View(list);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new TaskItem
        {
            DueDate = DateTime.Today.AddDays(1),
            Description = string.Empty
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TaskItem item)
    {
        TaskItemRules.Normalize(item, ModelState);
        if (!ModelState.IsValid)
        {
            return View(item);
        }

        item.Id = 0;
        _db.Tasks.Add(item);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var task = await _db.Tasks.FirstOrDefaultAsync(item => item.Id == id);
        if (task is null)
        {
            return NotFound();
        }

        return View(task);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TaskItem item)
    {
        if (id != item.Id)
        {
            ModelState.AddModelError(string.Empty, "Некорректный идентификатор задачи");
        }

        TaskItemRules.Normalize(item, ModelState);
        if (!ModelState.IsValid)
        {
            item.Id = id;
            return View(item);
        }

        var existing = await _db.Tasks.FirstOrDefaultAsync(task => task.Id == id);
        if (existing is null)
        {
            return NotFound();
        }

        existing.Title = item.Title;
        existing.Description = item.Description;
        existing.DueDate = item.DueDate;
        existing.IsDone = item.IsDone;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _db.Tasks.FirstOrDefaultAsync(task => task.Id == id);
        if (existing is not null)
        {
            _db.Tasks.Remove(existing);
            await _db.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
