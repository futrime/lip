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
        ResetUI();
        IsVisible = false;
        s_colors.Clear();
    }

    private async void IsRemoteCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        await CheckBoxAnimation(e.Value);
    }

    private async ValueTask CheckBoxAnimation(bool val)
    {
        View disableView = val ? _selectPathButton : _portEntry;
        View enableView = val ? _portEntry : _selectPathButton;

        if (disableView is null || enableView is null) return;

        disableView.IsEnabled = true;
        disableView.Opacity = 1;
        enableView.IsEnabled = false;
        enableView.Opacity = 0;

        List<Task> tasks = [_locationEntry.FadeTo(0, 150, Easing.CubicIn), disableView.FadeTo(0, 150, Easing.CubicIn)];
        if (val is false) tasks.Add(_passwordLayout.FadeTo(0, 150, Easing.CubicIn));

        await Task.WhenAll(tasks);
        disableView.IsEnabled = false;

        _locationEntry.Placeholder = val ? "IP Address" : "Location";
        if (val is false) _passwordLayout.IsVisible = false;
        else { _passwordLayout.Opacity = 0; _passwordLayout.IsVisible = true; }

        tasks = [_locationEntry.FadeTo(1, 150, Easing.CubicOut), enableView.FadeTo(1, 150, Easing.CubicOut)];
        if (val is true) tasks.Add(_passwordLayout.FadeTo(1, 150, Easing.CubicOut));

        enableView.IsEnabled = true;
        await Task.WhenAll(tasks);
    }

    private List<Entry> CheckInputs()
    {
        List<Entry> views = [];
        if (string.IsNullOrWhiteSpace(_nameEntry.Text)) views.Add(_nameEntry);
        if (_isRemoteCheckBox.IsChecked && string.IsNullOrWhiteSpace(_passwordEntry.Text)) views.Add(_passwordEntry);
        if (string.IsNullOrWhiteSpace(_locationEntry.Text)) views.Add(_locationEntry);
        if (_isRemoteCheckBox.IsChecked && string.IsNullOrWhiteSpace(_portEntry.Text)) views.Add(_portEntry);
        return views;
    }

    private static readonly Dictionary<Entry, Color> s_colors = [];

    private static async ValueTask MandatoryInputsAnimation(List<Entry> views)
    {
        foreach (Entry view in views)
        {
            s_colors.TryAdd(view, view.PlaceholderColor);
            view.PlaceholderColor = Colors.DarkRed;
            await view.ScaleTo(1.1, 100, Easing.CubicOut);
            await view.ScaleTo(1, 100, Easing.CubicIn);
        }
    }

    private void ResetUI()
    {
        foreach ((Entry view, Color color) in s_colors) view.PlaceholderColor = color;
        if (_isRemoteCheckBox.IsChecked is false) _isRemoteCheckBox.IsChecked = true;
    }

    private FileResult? _iconImage;

    private async void ServerIcon_Clicked(object sender, EventArgs e)
    {
        var options = new PickOptions { FileTypes = FilePickerFileType.Images };
        FileResult? result = await FilePicker.Default.PickAsync(options);
        if (result is null) return;
        _iconImage = result;

        Stream stream = await result.OpenReadAsync();
        _serverIcon.Source = ImageSource.FromStream(() => stream);
    }

    private void PortEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.NewTextValue) is false && e.NewTextValue.Any(c => char.IsDigit(c) is false))
            _portEntry.Text = e.OldTextValue;
    }

    private async void CancelButton_Clicked(object sender, EventArgs e)
    {
        await ExitAnimation();
        ServersPage.IsButtonsEnabled = true;
    }

    private void NameEntry_Completed(object sender, EventArgs e)
    {
        if (s_colors.TryGetValue(_nameEntry, out Color? color) && color != _nameEntry.PlaceholderColor)
            _nameEntry.PlaceholderColor = color;
    }

    private void LocationEntry_Completed(object sender, EventArgs e)
    {
        if (s_colors.TryGetValue(_locationEntry, out Color? color) && color != _nameEntry.PlaceholderColor)
            _nameEntry.PlaceholderColor = color;
    }

    private void PortEntry_Completed(object sender, EventArgs e)
    {
        if (s_colors.TryGetValue(_portEntry, out Color? color) && color != _nameEntry.PlaceholderColor)
            _nameEntry.PlaceholderColor = color;
    }

    private void PasswordEntry_Completed(object sender, EventArgs e)
    {
        if (s_colors.TryGetValue(_passwordEntry, out Color? color) && color != _nameEntry.PlaceholderColor)
            _nameEntry.PlaceholderColor = color;
    }

    private async void ConfirmButton_Clicked(object sender, EventArgs e)
    {
        List<Entry> views = CheckInputs();
        if (views.Count is not 0)
        {
            await MandatoryInputsAnimation(views);
            return;
        }
        if (ServersPage.Current is null) return;

        try
        {
            await ServersPage.CreateServerAsync(
               _nameEntry.Text,
               _locationEntry.Text,
               _isRemoteCheckBox.IsChecked ? _portEntry.Text : null,
               _isRemoteCheckBox.IsChecked ? _passwordEntry.Text : null,
               _iconImage,
               _color);
        }
        catch (Exception ex)
        {
            ServersPage.Current.InfoBar.Show(ex, containsStacktrace: true);
        }
        await ExitAnimation();
        ServersPage.IsButtonsEnabled = true;
    }

    private Color? _color;

    private void TextColorEntry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (Color.TryParse(_textColorEntry.Text, out Color? color))
            _color = color;
        else
            _color = null;

        _textColorBox.BackgroundColor = _color ?? Colors.Transparent;
    }
}
