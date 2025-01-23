namespace Lip.GUI.Pages.Servers;

public partial class AddServerButtonView : ContentView
{
    public AddServerButtonView()
    {
        InitializeComponent();
        ServersPage.ButtonsEnabledChanged += (s, e) => _button.IsEnabled = e;
    }

    private AddServerView? _addServerView;

    private async void Button_Pressed(object sender, EventArgs e)
    {
        await _button.ScaleTo(1.1, 100, Easing.CubicOut);
    }

    private void Button_Released(object sender, EventArgs e)
    {
        _button.ScaleTo(1, 100, Easing.CubicIn);
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        if (_addServerView is null)
        {
            if (ServersPage.Current is not null)
            {
                _addServerView = ServersPage.Current.AddServerView;
            }
            else
            {
                return;
            }
        }

        _addServerView.OnAddServerButtonViewClicked(this, EventArgs.Empty);
    }
}
