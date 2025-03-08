namespace Lip.GUI.Pages.LipRuntime;

public partial class LipRuntimeTaskView : ContentView
{
    private LipRuntimePage _page;

	public LipRuntimeTaskView(LipRuntimePage page)
	{
		InitializeComponent();

        _page = page;
	}

    private void Button_Clicked(object sender, EventArgs e)
    {
        Navigation.PushAsync(_page);
    }
}
