using TaskManager.Client.Models;
using TaskManager.Client.Services;

namespace TaskManager.Client;

internal sealed class MainForm : Form
{
    private readonly DataGridView _grid = new();
    private readonly TextBox _serverUrl = new();
    private readonly Label _status = new();
    private readonly Button _refreshButton = new();
    private readonly Button _addButton = new();
    private TaskApiClient? _client;

    public MainForm()
    {
        Text = "TaskManager";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(860, 520);
        Size = new Size(980, 620);
        Font = new Font("Segoe UI", 10F);

        var top = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 58,
            Padding = new Padding(12, 12, 12, 8),
            ColumnCount = 4
        };
        top.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        top.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        top.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        top.Controls.Add(new Label
        {
            Text = "URL сервера",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 6, 8, 0)
        }, 0, 0);

        _serverUrl.Dock = DockStyle.Fill;
        _serverUrl.Text = AppSettings.GetServerUrl();
        top.Controls.Add(_serverUrl, 1, 0);

        _refreshButton.Text = "Обновить";
        _refreshButton.AutoSize = true;
        _refreshButton.Margin = new Padding(8, 0, 0, 0);
        _refreshButton.Click += async (_, _) => await LoadTasksSafeAsync();
        top.Controls.Add(_refreshButton, 2, 0);

        _addButton.Text = "Добавить";
        _addButton.AutoSize = true;
        _addButton.Margin = new Padding(8, 0, 0, 0);
        _addButton.Click += OnAddClick;
        top.Controls.Add(_addButton, 3, 0);

        _grid.Dock = DockStyle.Fill;
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AutoGenerateColumns = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.RowHeadersVisible = false;
        _grid.BackgroundColor = Color.White;
        _grid.BorderStyle = BorderStyle.None;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.Columns.Add(TextColumn(nameof(TaskItem.Id), "Id", 40));
        _grid.Columns.Add(TextColumn(nameof(TaskItem.Title), "Название", 150));
        _grid.Columns.Add(TextColumn(nameof(TaskItem.Description), "Описание", 220));
        _grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(TaskItem.DueDate),
            HeaderText = "Срок",
            FillWeight = 80,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy" }
        });
        _grid.Columns.Add(new DataGridViewCheckBoxColumn
        {
            DataPropertyName = nameof(TaskItem.IsDone),
            HeaderText = "Готово",
            FillWeight = 50
        });

        _status.Dock = DockStyle.Bottom;
        _status.Height = 32;
        _status.TextAlign = ContentAlignment.MiddleLeft;
        _status.Padding = new Padding(12, 0, 12, 0);
        _status.Text = "Укажите URL сервера и нажмите «Обновить».";

        var gridHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(12, 0, 12, 8) };
        gridHost.Controls.Add(_grid);

        Controls.Add(gridHost);
        Controls.Add(_status);
        Controls.Add(top);

        Load += async (_, _) => await LoadTasksSafeAsync();
        FormClosed += (_, _) => _client?.Dispose();
    }

    private async void OnAddClick(object? sender, EventArgs e)
    {
        using var dialog = new AddTaskForm();
        if (dialog.ShowDialog(this) != DialogResult.OK || dialog.Task is null)
        {
            return;
        }

        try
        {
            SetBusy(true);
            var client = EnsureClient();
            await client.CreateAsync(dialog.Task);
            var tasks = await client.GetAllAsync();
            _grid.DataSource = tasks;
            _status.Text = $"Задача добавлена. Загружено задач: {tasks.Count}.";
        }
        catch (Exception exception)
        {
            ShowError(exception);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task LoadTasksSafeAsync()
    {
        try
        {
            SetBusy(true);
            var client = EnsureClient();
            var tasks = await client.GetAllAsync();
            _grid.DataSource = tasks;
            _status.Text = $"Загружено задач: {tasks.Count}. Сервер: {_serverUrl.Text.Trim()}";
        }
        catch (Exception exception)
        {
            _grid.DataSource = null;
            ShowError(exception);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private TaskApiClient EnsureClient()
    {
        var url = _serverUrl.Text.Trim();
        if (url.Length == 0)
        {
            throw new InvalidOperationException("Укажите URL сервера в поле настроек.");
        }

        try
        {
            AppSettings.SaveServerUrl(url);
        }
        catch (Exception exception)
        {
            _status.Text = "URL применён только для этой сессии: " + exception.Message;
        }

        _client?.Dispose();
        _client = new TaskApiClient(url);
        return _client;
    }

    private void SetBusy(bool busy)
    {
        UseWaitCursor = busy;
        _refreshButton.Enabled = !busy;
        _addButton.Enabled = !busy;
    }

    private void ShowError(Exception exception)
    {
        var message = exception switch
        {
            HttpRequestException http =>
                "Не удалось выполнить запрос. Проверьте, что сервер запущен и URL в настройках верный."
                + Environment.NewLine + Environment.NewLine + http.Message,
            TaskCanceledException => "Сервер не ответил за отведённое время.",
            _ => exception.Message
        };

        _status.Text = "Ошибка запроса";
        MessageBox.Show(this, message, "TaskManager", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private static DataGridViewTextBoxColumn TextColumn(string property, string header, float weight)
    {
        return new DataGridViewTextBoxColumn
        {
            DataPropertyName = property,
            HeaderText = header,
            FillWeight = weight
        };
    }
}
