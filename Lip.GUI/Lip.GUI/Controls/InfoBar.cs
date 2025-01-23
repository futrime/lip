#nullable disable
using Lip.GUI.Controls.Layouts;
using Lip.GUI.Pages.Servers;
using Microsoft.Maui.Controls.Shapes;

namespace Lip.GUI.Controls;

public enum InfoBarSeverity
{
    Informational,
    Success,
    Warning,
    Error
}

public partial class InfoBar : ContentView
{
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
            nameof(Title),
            typeof(string),
            typeof(InfoBar),
            null,
            propertyChanged: (bindable, oldValue, newValue) =>
            {
                var infoBar = (InfoBar)bindable;
                infoBar._titleLabel.Text = (string)newValue;
                infoBar._titleLabel.IsVisible = newValue is not null;
            });

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly BindableProperty MessageProperty = BindableProperty.Create(
        nameof(Message),
        typeof(string),
        typeof(InfoBar),
        null,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            var infoBar = (InfoBar)bindable;
            infoBar._messageLabel.Text = (string)newValue;
            infoBar._messageLabel.IsVisible = newValue is not null;
        });

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public static readonly BindableProperty SeverityProperty = BindableProperty.Create(
        nameof(Severity),
        typeof(InfoBarSeverity),
        typeof(InfoBar),
        InfoBarSeverity.Informational,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            var infoBar = (InfoBar)bindable;
            infoBar.OnSeverityChanged((InfoBarSeverity)newValue);
        });

    public InfoBarSeverity Severity
    {
        get => (InfoBarSeverity)GetValue(SeverityProperty);
        set => SetValue(SeverityProperty, value);
    }

    public static readonly BindableProperty IsClosableProperty = BindableProperty.Create(
        nameof(IsClosable),
        typeof(bool),
        typeof(InfoBar),
        true,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            var infoBar = (InfoBar)bindable;
            infoBar._closeButton.IsVisible = (bool)newValue;
        });

    public bool IsClosable
    {
        get => (bool)GetValue(IsClosableProperty);
        set => SetValue(IsClosableProperty, value);
    }

    public static new readonly BindableProperty ContentProperty = BindableProperty.Create(
        nameof(Content),
        typeof(View),
        typeof(InfoBar),
        null,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            var infoBar = (InfoBar)bindable;
            infoBar._contentView.Content = (View)newValue;
            infoBar._contentView.IsVisible = newValue is not null;
        });

    public new View Content
    {
        get => (View)GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public static readonly BindableProperty IsOpenProperty = BindableProperty.Create(
        nameof(IsOpen),
        typeof(bool),
        typeof(InfoBar),
        false,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            ((InfoBar)bindable).OnIsOpenChanged((bool)newValue);
        });


    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public static readonly BindableProperty IconSourceProperty = BindableProperty.Create(
        nameof(IconSource),
        typeof(ImageSource),
        typeof(InfoBar),
        null,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            var infoBar = (InfoBar)bindable;
            infoBar._iconImage.Source = (ImageSource)newValue;
            infoBar._iconModified = true;
        });

    public ImageSource IconSource
    {
        get => (ImageSource)GetValue(IconSourceProperty);
        set => SetValue(IconSourceProperty, value);
    }

    public static new readonly BindableProperty BackgroundColorProperty = BindableProperty.Create(
        nameof(BackgroundColor),
        typeof(Color),
        typeof(InfoBar),
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            var infoBar = (InfoBar)bindable;
            infoBar._border.BackgroundColor = (Color)newValue;
            infoBar._backgroundColorModified = true;
        });

    public new Color BackgroundColor
    {
        get => (Color)GetValue(BackgroundColorProperty);
        set => SetValue(BackgroundColorProperty, value);
    }

    public static new readonly BindableProperty BackgroundProperty = BindableProperty.Create(
        nameof(Background),
        typeof(Brush),
        typeof(InfoBar),
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            var infoBar = (InfoBar)bindable;
            infoBar._border.Background = (Brush)newValue;
        });

    public new Brush Background
    {
        get => (Brush)GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    private readonly Image _iconImage = new()
    {
        HeightRequest = 20,
        WidthRequest = 20,
        HorizontalOptions = LayoutOptions.Start,
        VerticalOptions = LayoutOptions.Start
    };

    private readonly Label _titleLabel = new()
    {

        TextColor = Colors.Black,
        FontAttributes = FontAttributes.Bold,
        FontSize = 16,
        LineBreakMode = LineBreakMode.CharacterWrap,
    };

    private readonly Label _messageLabel = new()
    {
        TextColor = Color.Parse("#1f1f1f"),
        FontSize = 14,
        LineBreakMode = LineBreakMode.CharacterWrap,
    };

    private readonly ContentView _contentView = new();

    private readonly Button _closeButton = new()
    {
        Background = Colors.Transparent,
        HorizontalOptions = LayoutOptions.End,
        VerticalOptions = LayoutOptions.Start,
        ImageSource = new FontImageSource()
        {
            FontFamily = "Segoe Fluent Icons",
            Glyph = "\uEDAE",
            Color = Colors.Black,
            Size = 20
        },
        HeightRequest = 16,
        WidthRequest = 16,
    };

    private readonly Border _border;

    public InfoBar()
    {
        VerticalOptions = LayoutOptions.End;
        HorizontalOptions = LayoutOptions.Center;

        Loaded += (sender, e) =>
        {
            IsVisible = false;
        };
        var layout = new ScrollView()
        {
            Content = new VerticalStackLayout()
            {
                Children =
                {
                    _titleLabel,
                    _messageLabel,
                    _contentView,
                }
            }
        };
        _closeButton.Clicked += CloseButton_Clicked;

        Grid.SetColumn(_iconImage, 0);
        Grid.SetColumn(layout, 1);
        Grid.SetColumn(_closeButton, 2);

        var @base = ((ContentView)this);
        @base.Content = _border = new Border()
        {
            VerticalOptions = LayoutOptions.Start,
            HorizontalOptions = LayoutOptions.Start,
            Padding = new(5),
            Stroke = Colors.Transparent,
            StrokeShape = new RoundRectangle()
            {
                CornerRadius = new CornerRadius(10)
            },
            StrokeThickness = 0,

            Content = new Grid()
            {
                VerticalOptions = LayoutOptions.Start,
                HorizontalOptions = LayoutOptions.Center,
                ColumnSpacing = 5,
                ColumnDefinitions =
                [
                    new ColumnDefinition(){ Width = new GridLength(1, GridUnitType.Auto) },
                    new ColumnDefinition(){ Width = new GridLength(1, GridUnitType.Star) },
                    new ColumnDefinition(){ Width = new GridLength(1, GridUnitType.Auto) },
                ],
                Children =
                {
                    _iconImage,
                    layout,
                    _closeButton
                }
            }
        };
    }


    private bool _iconModified = false;
    private bool _backgroundColorModified = false;
    private void OnSeverityChanged(InfoBarSeverity severity)
    {
        if (_iconModified is false)
        {
            Color color = SelectIconColor(severity);
            _iconImage.Source = SelectIconSource(severity, color);
        }

        if (_backgroundColorModified is false)
        {
            _border.BackgroundColor = SelectBackgroundColor(severity);
        }
    }

    private static FontImageSource SelectIconSource(InfoBarSeverity severity, Color color) => new()
    {
        FontFamily = "Segoe Fluent Icons",
        Glyph = severity switch
        {
            // InfoSolid
            InfoBarSeverity.Informational => "\uF167",
            // CompletedSolid
            InfoBarSeverity.Success => "\uEC61",
            // InfoSolid(different color)
            InfoBarSeverity.Warning => "\uF167",
            // StatusErrorFull
            InfoBarSeverity.Error => "\uEB90",
            _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null)
        },
        Color = color,
        Size = 20
    };

    private static Color SelectIconColor(InfoBarSeverity severity) => severity switch
    {
        InfoBarSeverity.Informational => Color.Parse("#0078d4"),
        InfoBarSeverity.Success => Color.Parse("#0f7b0f"),
        InfoBarSeverity.Warning => Color.Parse("#9d5d00"),
        InfoBarSeverity.Error => Color.Parse("#c42b1c"),
        _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null)
    };

    private static Color SelectBackgroundColor(InfoBarSeverity severity) => severity switch
    {
        InfoBarSeverity.Informational => Color.Parse("#fafafa"),
        InfoBarSeverity.Success => Color.Parse("#dff6dd"),
        InfoBarSeverity.Warning => Color.Parse("#fff4ce"),
        InfoBarSeverity.Error => Color.Parse("#fde7e9"),
        _ => throw new ArgumentOutOfRangeException(nameof(severity), severity, null)
    };

    private void OnIsOpenChanged(bool val)
    {
        Dispatcher.Dispatch(async () =>
        {
            if (val)
            {
                TranslationY = 10;
                Opacity = 0;
                IsVisible = true;
                await Task.WhenAll
                ([
                    this.TranslateTo(0, 0, 250, Easing.CubicOut),
                    this.FadeTo(1, 250, Easing.CubicOut)
                ]);
            }
            else
            {
                Closing?.Invoke(this, EventArgs.Empty);
                await this.FadeTo(0, 250, Easing.CubicOut);
                IsVisible = false;
                Closed?.Invoke(this, EventArgs.Empty);
            }
        });
    }

    private void CloseButton_Clicked(object sender, EventArgs e)
    {
        _cts?.Cancel();
    }

    public event EventHandler Closing;
    public event EventHandler Closed;

    private record struct InfoBarTask(
        ManualResetEvent Mre,
        string Title,
        string Message,
        InfoBarSeverity Severity,
        TimeSpan Interval,
        View Content,
        Action Completed,
        bool IsClosable,
        CancellationTokenSource source);

    private bool _isInfoBarTaskHandlerRunning = false;
    private readonly Queue<InfoBarTask> _infoBarTaskQueue = new();
    private CancellationTokenSource _cts;

    private void ShowInfoBar(InfoBarTask task)
    {
        Dispatcher.Dispatch(() =>
        {
            Title = task.Title;
            Message = task.Message;
            Severity = task.Severity;
            IsClosable = task.IsClosable;
            Content = task.Content;

            void set(object sender, object e)
            {
                Closed -= set;
                task.Mre.Set();
            }
            Closed += set;

            IsOpen = true;
        });
    }

    private void CloseInfoBar(ManualResetEvent mre)
    {
        IsOpen = false;

        void task(object sender, object e)
        {
            Closed -= task;
            mre.Set();
        }
        Closed += task;
    }

    private void StartInfoBarTaskHandler()
        => Task.Run(async () =>
        {
            InfoBarTask task;
            bool dequeued;

            _isInfoBarTaskHandlerRunning = true;
        LOOP:
            lock (_infoBarTaskQueue)
            {
                dequeued = _infoBarTaskQueue.TryDequeue(out task);
            }

            if (dequeued)
            {
                _cts = task.source;

                Dispatcher.Dispatch(() => ShowInfoBar(task));

                try
                {
                    await Task.Delay(task.Interval, task.source.Token);
                }
                catch (TaskCanceledException) { }

                _cts = null;

                task.Mre.Reset();
                Dispatcher.Dispatch(() => CloseInfoBar(task.Mre));
                task.Mre.WaitOne();
                task.Completed?.Invoke();
                task.Mre.Dispose();

                goto LOOP;
            }
            _isInfoBarTaskHandlerRunning = false;
        });

    private CancellationTokenSource ShowInternal(
        string title,
        string message,
        InfoBarSeverity severity,
        TimeSpan interval = default,
        bool isClosable = true,
        View barContent = null,
        Action completed = null)
    {
        CancellationTokenSource source = new();
        Task.Run(() =>
        {
            var mre = new ManualResetEvent(false);
            _infoBarTaskQueue.Enqueue(new(
                mre,
                title,
                message,
                severity,
                interval,
                barContent,
                completed,
                isClosable,
                source));
            if (_isInfoBarTaskHandlerRunning is false)
                StartInfoBarTaskHandler();
            mre.WaitOne();
        });

        return source;
    }

    private async ValueTask<CancellationTokenSource> ShowInternalAsync(
        string title,
        string message,
        InfoBarSeverity severity,
        TimeSpan interval = default,
        bool isClosable = true,
        View barContent = null)
    {
        CancellationTokenSource source = new();
        await Task.Run(() =>
        {
            var mre = new ManualResetEvent(false);
            _infoBarTaskQueue.Enqueue(new(
                mre,
                title,
                message,
                severity,
                interval,
                barContent,
                null,
                isClosable,
                source));
            if (_isInfoBarTaskHandlerRunning is false)
                StartInfoBarTaskHandler();
            mre.WaitOne();
        });
        return source;
    }

    public async ValueTask<CancellationTokenSource> ShowAsync(
        string title = null,
        string message = null,
        InfoBarSeverity severity = InfoBarSeverity.Informational,
        TimeSpan interval = default,
        bool isClosable = true,
        View barContent = null)
    {
        if (interval == default)
            interval = TimeSpan.FromSeconds(3);

        return await ShowInternalAsync(
            title,
            message,
            severity,
            interval,
            isClosable,
            barContent);
    }

    public async ValueTask<CancellationTokenSource> ShowAsync(
        Exception ex,
        bool containsStacktrace = false,
        InfoBarSeverity severity = InfoBarSeverity.Error,
        TimeSpan interval = default,
        bool isClosable = true,
        View barContent = null,
        CancellationToken cancellationToken = default)
    {
        if (interval == default)
            interval = TimeSpan.FromSeconds(5);

        return await ShowAsync(
            ex.GetType().Name,
            containsStacktrace ? ex.ToString() : ex.Message,
            severity,
            interval,
            isClosable,
            barContent);
    }

    public CancellationTokenSource Show(
        string title = null,
        string message = null,
        InfoBarSeverity severity = InfoBarSeverity.Informational,
        TimeSpan interval = default,
        bool isClosable = true,
        View barContent = null,
        Action completed = null)
    {
        if (interval == default)
            interval = TimeSpan.FromSeconds(3);

        return ShowInternal(
            title,
            message,
            severity,
            interval,
            isClosable,
            barContent,
            completed);
    }

    public CancellationTokenSource Show(
        Exception ex,
        bool containsStacktrace = false,
        InfoBarSeverity severity = InfoBarSeverity.Error,
        TimeSpan interval = default,
        bool isClosable = true,
        View barContent = null,
        Action completed = null)
    {
        if (interval == default)
            interval = TimeSpan.FromSeconds(5);

        return Show(
            ex.GetType().Name,
            containsStacktrace ? ex.ToString() : ex.Message,
            severity,
            interval,
            isClosable,
            barContent,
            completed);
    }
}
