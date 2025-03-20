using System.Collections.ObjectModel;
using Lip.GUI.Pages.Tasks;
using Microsoft.Maui.Adapters;

namespace Lip.GUI.Pages.LipRuntime;

public partial class LipRuntimePage : ContentPage, ITask
{

    public ObservableCollection<string> Items { get; } = [];


    public event EventHandler<TaskEventArgs>? TaskCompleted;

    public IView? TaskView { get; } 

    public LipRuntimePage()
    {
        InitializeComponent();

        TaskView = new LipRuntimeTaskView(this);

        _listView.Adapter = new ObservableCollectionAdapter<string>(Items);
    }

    public void Start()
    {

    }
}
