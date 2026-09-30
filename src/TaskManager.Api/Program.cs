using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.WebEncoders;
using TaskManager.Api.Data;
using TaskManager.Api.Logging;
using TaskManager.Api.Options;
using TaskManager.Api.Services;

var builder = WebApplication.CreateBuilder(args);

var logPath = Path.Combine(builder.Environment.ContentRootPath, "logs", "taskmanager.log");
builder.Logging.AddProvider(new FileLoggerProvider(logPath));

builder.Services.AddControllersWithViews(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
    var messages = options.ModelBindingMessageProvider;
    messages.SetValueMustNotBeNullAccessor(_ => "Поле обязательно для заполнения");
    messages.SetMissingBindRequiredValueAccessor(name => $"Не заполнено поле {name}");
    messages.SetAttemptedValueIsInvalidAccessor((value, name) => $"Некорректное значение «{value}» для поля {name}");
    messages.SetNonPropertyAttemptedValueIsInvalidAccessor(value => $"Некорректное значение «{value}»");
    messages.SetUnknownValueIsInvalidAccessor(name => $"Некорректное значение поля {name}");
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<ExportOptions>(builder.Configuration.GetSection(ExportOptions.SectionName));
builder.Services.Configure<WebEncoderOptions>(options =>
{
    options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic);
});
builder.Services.Configure<JsonOptions>(options =>
{
    options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic);
});
builder.Services.AddScoped<ITaskExportService, TaskExportService>();
builder.Services.AddHostedService<TaskExportBackgroundService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    DbSeeder.Seed(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
