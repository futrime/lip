namespace Lip.GUI.Pages.Servers;

public partial class ServersPage : ContentPage
{
    public ServersPage()
    {
        InitializeComponent();
    }

    internal AddServerView AddServerView => _addServerView;

    internal static ServersPage? Current { get; private set; }


    private static bool s_isButtonsEnabled = true;

    public static bool IsButtonsEnabled
    {
        get => s_isButtonsEnabled;
        set
        {
            ButtonsEnabledChanged?.Invoke(null, value);
            s_isButtonsEnabled = value;
        }
    }

    public static event EventHandler<bool>? ButtonsEnabledChanged;

    private void ContentPage_Loaded(object sender, EventArgs e)
        => Current = this;
}
