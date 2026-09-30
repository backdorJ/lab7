using TaskManager.Client.Models;

namespace TaskManager.Client;

internal sealed class AddTaskForm : Form
{
    private readonly TextBox _title = new();
    private readonly TextBox _description = new();
    private readonly DateTimePicker _dueDate = new();
    private readonly CheckBox _isDone = new();

    public TaskItem? Task { get; private set; }

    public AddTaskForm()
    {
        Text = "Новая задача";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;
        MaximizeBox = false;
        ClientSize = new Size(480, 340);
        Font = new Font("Segoe UI", 10F);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            ColumnCount = 2,
            RowCount = 5
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 110));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));

        _title.Dock = DockStyle.Fill;
        _description.Multiline = true;
        _description.ScrollBars = ScrollBars.Vertical;
        _description.Dock = DockStyle.Fill;
        _dueDate.Format = DateTimePickerFormat.Short;
        _dueDate.Value = DateTime.Today.AddDays(1);
        _dueDate.Dock = DockStyle.Left;
        _dueDate.Width = 160;
        _isDone.Text = "Выполнена";
        _isDone.AutoSize = true;

        AddLabeled(layout, 0, "Название", _title);
        AddLabeled(layout, 1, "Описание", _description);
        AddLabeled(layout, 2, "Срок", _dueDate);
        layout.Controls.Add(_isDone, 1, 3);

        var buttons = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft
        };
        var ok = new Button { Text = "Добавить", AutoSize = true };
        var cancel = new Button { Text = "Отмена", DialogResult = DialogResult.Cancel, AutoSize = true };
        ok.Click += OnAddClick;
        buttons.Controls.Add(ok);
        buttons.Controls.Add(cancel);
        layout.Controls.Add(buttons, 0, 4);
        layout.SetColumnSpan(buttons, 2);

        AcceptButton = ok;
        CancelButton = cancel;
        Controls.Add(layout);
    }

    private void OnAddClick(object? sender, EventArgs e)
    {
        var title = _title.Text.Trim();
        var description = _description.Text.Trim();

        if (title.Length == 0)
        {
            ShowWarning("Укажите название задачи.");
            return;
        }

        if (title.Length > 200)
        {
            ShowWarning("Название должно содержать не больше 200 символов.");
            return;
        }

        if (description.Length > 2000)
        {
            ShowWarning("Описание должно содержать не больше 2000 символов.");
            return;
        }

        Task = new TaskItem
        {
            Title = title,
            Description = description,
            DueDate = _dueDate.Value.Date,
            IsDone = _isDone.Checked
        };
        DialogResult = DialogResult.OK;
    }

    private void ShowWarning(string message)
    {
        MessageBox.Show(this, message, "TaskManager", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    private static void AddLabeled(TableLayoutPanel layout, int row, string caption, Control control)
    {
        layout.Controls.Add(new Label
        {
            Text = caption,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 8, 8, 0)
        }, 0, row);
        layout.Controls.Add(control, 1, row);
    }
}
