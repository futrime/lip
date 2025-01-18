namespace Lip.GUI.Pages.Servers;

public partial class AddServerView : ContentView
{
    public AddServerView()
    {
        InitializeComponent();
    }

    public async void OnAddServerButtonViewClicked(AddServerButtonView sender, EventArgs e)
    {
        ServersPage.IsButtonsEnabled = false;

        await EntryAnimation();
    }


    private async ValueTask EntryAnimation()
    {

        Opacity = 0;
        TranslationY = 20;
        _addIconImage.Opacity = 0;
        _addServerText.Opacity = 0;


        IsVisible = true;

        await Task.WhenAll
        ([
            this.TranslateTo(0, 0, 250, Easing.CubicOut),
            this.FadeTo(1, 250, Easing.CubicOut),
            _addIconImage.FadeTo(1, 500, Easing.CubicOut),
            _addServerText.FadeTo(1, 500, Easing.CubicOut),
        ]);
    }

    private async ValueTask ExitAnimation()
    {
        await this.FadeTo(0, 250, Easing.CubicIn);
        await ResetUI();
        IsVisible = false;
    }

    private async void IsRemoteCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        await CheckBoxAnimation(e.Value);
    }

    private async ValueTask CheckBoxAnimation(bool val)
    {

        View disableView = val ? _selectPathButton : _portEntry;
        View enableView = val ? _portEntry : _selectPathButton;

        if(disableView is null || enableView is null) return;

        disableView.IsEnabled = true;
        disableView.Opacity = 1;
        enableView.IsEnabled = false;
        enableView.Opacity = 0;

        await Task.WhenAll
        ([
            _locationEntry.FadeTo(0, 150, Easing.CubicIn),
            disableView.FadeTo(0, 150, Easing.CubicIn)
        ]);
        disableView.IsEnabled = false;

        _locationEntry.Placeholder = val ? "IP Address" : "Location";

        enableView.IsEnabled = true;
        await Task.WhenAll
        ([
            _locationEntry.FadeTo(1, 150, Easing.CubicOut),
            enableView.FadeTo(1, 150, Easing.CubicOut)
        ]);
    }

    private async void CancelButton_Clicked(object sender, EventArgs e)
    {
        await ExitAnimation();
        ServersPage.IsButtonsEnabled = true;
    }

    private async void ConfirmButton_Clicked(object sender, EventArgs e)
    {
        List<Entry> views = CheckInputs();
        if (views.Count is not 0)
        {
            await MandatoryInputsAnimation(views);
            return;
        }
    }

    private List<Entry> CheckInputs()
    {
        List<Entry> views = [];
        if (string.IsNullOrWhiteSpace(_nameEntry.Text)) views.Add(_nameEntry);
        if (string.IsNullOrWhiteSpace(_locationEntry.Text)) views.Add(_locationEntry);
        if (_isRemoteCheckBox.IsChecked && string.IsNullOrWhiteSpace(_portEntry.Text)) views.Add(_portEntry);
        return views;
    }

    private static Dictionary<Entry, Color> colors = [];

    private async ValueTask MandatoryInputsAnimation(List<Entry> views)
    {
        foreach (Entry view in views)
        {
            colors.TryAdd(view, view.PlaceholderColor);
            view.PlaceholderColor = Colors.DarkRed;
            await view.ScaleTo(1.1, 100, Easing.CubicOut);
            await view.ScaleTo(1, 100, Easing.CubicIn);
        }
    }

    private async ValueTask ResetUI()
    {
        foreach ((Entry view, Color color) in colors) view.PlaceholderColor = color;
        if (_isRemoteCheckBox.IsChecked) _isRemoteCheckBox.IsChecked = false;
    }

    private void NameEntry_Completed(object sender, EventArgs e)
    {
        if(colors.TryGetValue(_nameEntry, out Color? color) && color != _nameEntry.PlaceholderColor)
            _nameEntry.PlaceholderColor = color;
    }

    private void LocationEntry_Completed(object sender, EventArgs e)
    {
        if (colors.TryGetValue(_locationEntry, out Color? color) && color != _nameEntry.PlaceholderColor)
            _nameEntry.PlaceholderColor = color;
    }

    private void PortEntry_Completed(object sender, EventArgs e)
    {
        if (colors.TryGetValue(_portEntry, out Color? color) && color != _nameEntry.PlaceholderColor)
            _nameEntry.PlaceholderColor = color;
    }
}
