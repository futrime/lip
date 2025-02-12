using System.Net;
using Lip.Connection;

namespace Lip.GUI.Pages.Servers;

public partial class ServerView : ContentView
{
    private static FontImageSource DefualtServerIcon => new()
    {
        FontFamily = "Segoe Fluent Icons",
        Glyph = "\uE91B",
        Color = Application.Current?.RequestedTheme switch
        {
            AppTheme.Light => Colors.Magenta,
            AppTheme.Dark => Colors.White,
            _ => Colors.Magenta
        } ?? Colors.Magenta
    };

    public static readonly BindableProperty ServerNameProperty = BindableProperty.Create(
        nameof(ServerName),
        typeof(string),
        typeof(ServerView),
        string.Empty,
        validateValue: (bindable, value) => string.IsNullOrWhiteSpace((string)value) is false,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            var view = (ServerView)bindable;
            view._serverNameLabel.Text = (string)newValue;
        });

    public static readonly BindableProperty ServerHostProperty = BindableProperty.Create(
        nameof(ServerHost),
        typeof(string),
        typeof(ServerView),
        string.Empty,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            var view = (ServerView)bindable;
            view._serverLocationLabel.Text = (string)newValue;
        });

    public static readonly BindableProperty ServerPasswordProperty = BindableProperty.Create(
        nameof(ServerHost),
        typeof(string),
        typeof(ServerView),
        string.Empty);

    public static readonly BindableProperty ServerPortProperty = BindableProperty.Create(
        nameof(ServerPort),
        typeof(int?),
        typeof(ServerView),
        null,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            var view = (ServerView)bindable;
            view._serverPortLabel.Text = (int?)newValue is not null ? $"{(int)newValue}" : "Local";
        });

    public static readonly BindableProperty ClientPortProperty = BindableProperty.Create(
    nameof(ClientPort),
    typeof(int?),
    typeof(ServerView),
    null);

    public static readonly BindableProperty ServerIconIdProperty = BindableProperty.Create(
        nameof(ServerIconId),
        typeof(Guid?),
        typeof(ServerView),
        null,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            var view = (ServerView)bindable;
            var id = (Guid?)newValue;
            view._serverIcon.Source = id is null ? DefualtServerIcon :
            MauiProgram.Config!.LoadServerIcon(id.Value);
        });

    public static readonly BindableProperty TextColorProperty = BindableProperty.Create(
    nameof(ServerHost),
    typeof(Color),
    typeof(ServerView),
    null,
    propertyChanged: (bindable, oldValue, newValue) =>
    {
        var view = (ServerView)bindable;
        var color = (Color?)newValue;

        if (color is null) return;

        view._serverNameLabel.TextColor = color;
        view._serverLocationLabel.TextColor = color;
        view._serverPortLabel.TextColor = color;
    });


    public string ServerName
    {
        get => (string)GetValue(ServerNameProperty);
        set => SetValue(ServerNameProperty, value);
    }

    public string ServerHost
    {
        get => (string)GetValue(ServerHostProperty);
        set => SetValue(ServerHostProperty, value);
    }

    public int? ServerPort
    {
        get => (int?)GetValue(ServerPortProperty);
        set => SetValue(ServerPortProperty, value);
    }

    public int? ClientPort
    {
        get => (int?)GetValue(ClientPortProperty);
        set => SetValue(ClientPortProperty, value);
    }

    public Guid? ServerIconId
    {
        get => (Guid?)GetValue(ServerIconIdProperty);
        set => SetValue(ServerIconIdProperty, value);
    }

    public string? ServerPassword
    {
        get => (string?)GetValue(ServerPasswordProperty);
        set => SetValue(ServerPasswordProperty, value);
    }

    public Color? TextColor
    {
        get => (Color?)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }

    public ServerView()
    {
        InitializeComponent();
    }

    private bool _verified = false;

    private void ContentView_Loaded(object sender, EventArgs e)
    {
        _button.IsEnabled = false;

        if (_verified) return;
        _verified = true;
        Task.Run(VerifyRemoteServer);
    }

    public Connection.Connection? Connection { get; private set; } = null;

    private enum VerifyStatus { Verifying, Verified, Failed }

    private async ValueTask<bool> VerifyRemoteServer()
    {
        if (ServerPort is null)
        {
            await VerifyAnimation(VerifyStatus.Failed);
            return false;
        }

        CancellationTokenSource cancellationTokenSource = new();
        Task task = VerifyAnimation(VerifyStatus.Verifying, cancellationTokenSource.Token).AsTask();

        try
        {
            Connection = new Connection.Connection(
                ConnectionMode.Client,
                ServerPassword ?? "",
                IPAddress.Any,
                ClientPort!.Value);

            //await Task.Delay(10000);
            await Connection.ConnectToAsync(new(IPAddress.Parse(ServerHost), ServerPort!.Value));
        }
        catch (Exception ex)
        {
            await CancelVerifyAnimation(task, cancellationTokenSource);
            await VerifyAnimation(VerifyStatus.Failed);
            ServersPage.Current?.InfoBar.Show(ex, containsStacktrace: true);
            return false;
        }

        await CancelVerifyAnimation(task, cancellationTokenSource);
        await VerifyAnimation(VerifyStatus.Verified);

        Dispatcher.Dispatch(() => _button.IsEnabled = true);
        return true;
    }

    private static async ValueTask CancelVerifyAnimation(Task task, CancellationTokenSource source)
    {
        source.Cancel();
        await task;
    }

    private async ValueTask VerifyAnimation(VerifyStatus status, CancellationToken? token = null)
    {
        await this.ExecuteInUIThreadAsync(task: async () =>
        {
            switch (status)
            {
                case VerifyStatus.Verifying:
                    await Task.WhenAll
                    ([
                        _statusImage.FadeTo(0, 1000, Easing.CubicInOut),
                        _statusImage.ScaleTo(0, 1000, Easing.CubicInOut),
                        _statusImage.RelRotateTo(-360, 1000, Easing.CubicInOut),
                    ]);
                    _statusImage.Source = CreateVerifyStatusIcon(VerifyStatus.Verifying);
                    await Task.WhenAll
                    ([
                        _statusImage.FadeTo(1, 1000, Easing.CubicInOut),
                        _statusImage.ScaleTo(1, 1000, Easing.CubicInOut),
                        _statusImage.RelRotateTo(-360, 1000, Easing.CubicInOut),
                    ]);
                    while (token is not null && token.Value.IsCancellationRequested is false)
                    {
                        await _statusImage.RelRotateTo(-360, 1000, Easing.CubicInOut);
                    }
                    break;

                case VerifyStatus.Verified:

                    await Task.WhenAll
                    ([
                        _statusImage.FadeTo(0, 1000, Easing.CubicInOut),
                        _statusImage.ScaleTo(0, 1000, Easing.CubicInOut),
                        _statusImage.RelRotateTo(-360, 1000, Easing.CubicInOut),
                    ]);
                    _statusImage.Source = CreateVerifyStatusIcon(VerifyStatus.Verified);
                    await Task.WhenAll
                    ([
                        _statusImage.FadeTo(1, 1000, Easing.CubicInOut),
                        _statusImage.ScaleTo(1, 1000, Easing.CubicInOut),
                        _statusImage.RelRotateTo(-360, 1000, Easing.CubicInOut),
                    ]);
                    break;

                case VerifyStatus.Failed:

                    await Task.WhenAll
                    ([
                        _statusImage.FadeTo(0, 1000, Easing.CubicInOut),
                        _statusImage.ScaleTo(0, 1000, Easing.CubicInOut),
                        _statusImage.RelRotateTo(-360, 1000, Easing.CubicInOut),
                    ]);
                    _statusImage.Source = CreateVerifyStatusIcon(VerifyStatus.Failed);
                    await Task.WhenAll
                    ([
                        _statusImage.FadeTo(1, 1000, Easing.CubicInOut),
                        _statusImage.ScaleTo(1, 1000, Easing.CubicInOut),
                        _statusImage.RelRotateTo(-360, 1000, Easing.CubicInOut),
                    ]);
                    break;
            }
        });
    }

    private static FontImageSource CreateVerifyStatusIcon(VerifyStatus status) => new()
    {
        FontFamily = "Segoe Fluent Icons",
        Glyph = status switch
        {
            // icon: UpdateRestore
            VerifyStatus.Verifying => "\uE777",
            // icon: CheckMark
            VerifyStatus.Verified => "\uE73E",
            // icon: ErrorBadge12
            VerifyStatus.Failed => "\uEDAE",
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        },
        Color = status switch
        {
            VerifyStatus.Verifying => Colors.LightBlue,
            VerifyStatus.Verified => Colors.LightGreen,
            VerifyStatus.Failed => Color.Parse("#F297ED"),
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        }
    };
}
