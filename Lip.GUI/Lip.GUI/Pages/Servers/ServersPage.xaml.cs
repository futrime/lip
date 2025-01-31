using System.Collections.ObjectModel;
using System.Net;
using Lip.GUI.Controls;
using Lip.GUI.Themes;

namespace Lip.GUI.Pages.Servers;


public partial class ServersPage : ContentPage, IBackgroundImageHandler
{
    public ImageSource BackgroundImage
    {
        get => _background.Source;
        set => _background.Source = value;
    }

    public ServersPage()
    {
        InitializeComponent();
        MauiProgram.Config.PropertyChanged += Config_PropertyChanged;
        MauiProgram.Config.Servers.CollectionChanged += Servers_CollectionChanged;
    }

    private static ObservableCollection<ServerInfo>? s_servers = null;

    private async void Servers_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        await UpdateAllServersAsync(MauiProgram.Config.Servers);
    }

    private void Config_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(MauiProgram.Config.Servers))
        {
            if (s_servers is not null)
                s_servers.CollectionChanged -= Servers_CollectionChanged;

            s_servers = MauiProgram.Config.Servers;
            s_servers.CollectionChanged += Servers_CollectionChanged;
        }
    }

    internal AddServerView AddServerView => _addServerView;

    internal static ServersPage? Current { get; private set; }

    public static bool IsButtonsEnabled
    {
        get;
        set
        {
            ButtonsEnabledChanged?.Invoke(null, value);
            field = value;
        }
    } = true;

    public static event EventHandler<bool>? ButtonsEnabledChanged;

    private void ContentPage_Loaded(object sender, EventArgs e)
        => Current = this;

    public static async ValueTask CreateServerAsync(
        string name,
        string location,
        string? portStr,
        string? password,
        FileResult? iconImage,
        Color? textColor)
    {
        if (MauiProgram.Config.Servers.Any(s => s.ServerName == name))
            throw new Exception("Server with the same name already exists.");

        ServerInfo? server = new()
        {
            ServerName = name,
            ServerHost = location.Trim() is "localhost" ? "127.0.0.1" : location,
            ServerPort = null,
            ServerIconId = null,
            ServerPassword = null,
            TextColor = textColor?.ToHex()
        };

        if (portStr is not null)
        {
            _ = IPAddress.Parse(server.ServerHost);
            server.ServerPort = int.Parse(portStr);
            server.ServerPassword = password;
        }
        else
        {
            throw new NotImplementedException("Local servers are not supported yet.");
        }

        if (iconImage is not null)
        {
            Guid id = Guid.NewGuid();
            while (File.Exists(Path.Combine(MauiProgram.IconsDirectory, $"{id}.png")))
            {
                id = Guid.NewGuid();
            }
            using Stream stream = await iconImage.OpenReadAsync();
            using FileStream fileStream = new(Path.Combine(MauiProgram.IconsDirectory, $"{id}.png"), FileMode.Create);
            await stream.CopyToAsync(fileStream);
            server.ServerIconId = id;
        }

        MauiProgram.Config.Servers.Add(server);
    }

    private readonly Dictionary<string, ServerView> _serverViews = [];

    private async ValueTask UpdateAllServersAsync(IEnumerable<ServerInfo> servers)
    {
        await Task.Run(() =>
        {
            foreach (ServerInfo server in servers)
            {
                if (_serverViews.ContainsKey(server.ServerName) is false)
                {
                    var view = new ServerView()
                    {
                        ServerName = server.ServerName,
                        ServerHost = server.ServerHost,
                        ServerPort = server.ServerPort,
                        ServerIconId = server.ServerIconId,
                        ServerPassword = server.ServerPassword,
                        TextColor = Color.Parse(server.TextColor ?? "#00000000")
                    };

                    _serverViews.Add(server.ServerName, view);

                    Dispatcher.Dispatch(() => _viewsLayout.Children.Add(view));
                }
            }
        }).ConfigureAwait(false);
    }

    public InfoBar InfoBar => _infoBar;
}
