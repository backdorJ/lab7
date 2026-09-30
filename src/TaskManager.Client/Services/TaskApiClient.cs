using System.Net.Http.Json;
using System.Text.Json;
using TaskManager.Client.Models;

namespace TaskManager.Client.Services;

public sealed class TaskApiClient : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly HttpClient _http;

    public TaskApiClient(string baseUrl)
    {
        if (!Uri.TryCreate(baseUrl.Trim(), UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("Укажите абсолютный URL сервера, например http://localhost:5080");
        }

        _http = new HttpClient
        {
            BaseAddress = new Uri(uri.ToString().TrimEnd('/') + "/"),
            Timeout = TimeSpan.FromSeconds(20)
        };
    }

    public async Task<List<TaskItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _http.GetAsync("api/tasks", cancellationToken);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<List<TaskItem>>(JsonOptions, cancellationToken)
            ?? new List<TaskItem>();
    }

    public async Task<TaskItem> CreateAsync(TaskItem item, CancellationToken cancellationToken = default)
    {
        using var response = await _http.PostAsJsonAsync("api/tasks", item, JsonOptions, cancellationToken);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<TaskItem>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Сервер вернул пустой ответ");
    }

    public void Dispose() => _http.Dispose();

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync();
        if (body.Length > 800)
        {
            body = body[..800];
        }

        throw new HttpRequestException(
            $"Сервер вернул {(int)response.StatusCode} ({response.ReasonPhrase}). {body}");
    }
}
