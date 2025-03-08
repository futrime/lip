namespace Lip.GUI.Pages.Tasks;
public interface ITask
{
    public IView? TaskView { get; }


    public event EventHandler<TaskEventArgs>? TaskCompleted;

    public void Start();
}
