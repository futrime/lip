using Lip.Connection.Network.Packets.CustomOperation;
using Lip.GUI.Pages.Bedrinth;
using Lip.GUI.Pages.Servers;

namespace Lip.GUI.Pages.PackageDetails;

public partial class PackageInstallationItemView
    : ContentView
{
    private readonly ServerView _view;
    private readonly BedrinthItem _item;
    private readonly PackageInstallationView _packageInstallationView;

    public PackageInstallationItemView(PackageInstallationView installationView, ServerView view, BedrinthItem item)
    {
        _view = view;
        _item = item;
        _packageInstallationView = installationView;

        InitializeComponent();

        _name.Text = view.ServerName;
    }

    private void ContentView_Loaded(object sender, EventArgs e)
    {
        if (_view.Connection is null ||
            _view.Connection.Connected is false ||
            _view.Connection.Verified is false)
        {
            SetStatus(Status.TestFailed);
            return;
        }

        SetStatus(Status.Testing);

        Task.Run(TestPackageInstallation);
    }

    private async ValueTask TestPackageInstallation()
    {
        await _view.Connection!.SendPacketAsync<CustomOperationPackets, TestPackageInstalledPacket>(
            CustomOperationPackets.TestPackageInstalled, new() { Identifier = _item.Identifier });

        var response = await _view.Connection.RequestPacketAsync<CustomOperationPackets, TestPackageInstalledResponsePacket>(
            CustomOperationPackets.TestPackageInstalledResponse);

        if (response.Result is null)
        {
            SetStatus(Status.NotInstalled);
            return;
        }
        else
        {
            SetStatus(Status.Installed);
            _version.Text = response.Result.Specifier.Version.ToString();
        }
    }

    private enum Status
    {
        Installed,
        NotInstalled,
        Testing,
        TestFailed
    }

    private void SetStatus(Status status)
    {
        Dispatcher.Dispatch(() =>
        {
            switch (status)
            {
                case Status.Installed:
                    _activityIndicator.IsRunning = false;
                    _name.IsEnabled = true;
                    _version.IsEnabled = true;
                    _checkbox.IsEnabled = true;
                    _checkbox.IsChecked = true;
                    break;

                case Status.NotInstalled:
                    _activityIndicator.IsRunning = false;
                    _name.IsEnabled = true;
                    _version.IsEnabled = true;
                    _checkbox.IsEnabled = true;
                    _checkbox.IsChecked = false;
                    break;

                case Status.Testing:
                    _activityIndicator.IsRunning = true;
                    _name.IsEnabled = false;
                    _version.IsEnabled = false;
                    _checkbox.IsEnabled = false;
                    _checkbox.IsChecked = false;
                    break;

                case Status.TestFailed:
                    _activityIndicator.IsRunning = false;
                    _name.IsEnabled = false;
                    _version.IsEnabled = false;
                    _checkbox.IsEnabled = false;
                    _checkbox.IsChecked = false;
                    break;
            }
        });
    }

    private void Checkbox_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        CheckBoxChanged = true;
        _packageInstallationView.ItemViewCheckBoxChanged(this, EventArgs.Empty);
    }

    public bool CheckBoxChanged { get; private set; }

    public bool CheckBoxValue => _checkbox.IsChecked;
}
