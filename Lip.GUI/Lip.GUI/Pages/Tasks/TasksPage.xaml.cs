
namespace Lip.GUI.Pages.Tasks;

public partial class TasksPage : ContentPage
{
    public static TasksPage? Current { get; private set; }

    public TasksPage()
    {
        InitializeComponent();
        Current = this;
    }

    public void CommitTask(ITask task)
    {
        task.TaskCompleted += Task_Completed;
        _wrapLayout.Children.Add(task.TaskView);
    }

    private void Task_Completed(object? sender, TaskEventArgs e)
    {
        if (sender is ITask task)
        {
            task.TaskCompleted -= Task_Completed;
            _wrapLayout.Children.Remove(task.TaskView);
        }
    }
}
