using System.Collections.ObjectModel;
using Bedrinth;

namespace Lip.GUI.Pages.Bedrinth;

public class BedrinthItem
{
    public required string Name { get; set; }

    public required string Description { get; set; }
}

public partial class BedrinthPage : ContentPage
{
    public BedrinthPage()
    {
        InitializeComponent();
    }

    private readonly BedrinthServicesProvider _bedrinthServicesProvider = new();

    private int _pageIndex = 1;

    private readonly ActivityIndicator _activityIndicator = new() { IsRunning = true };

    public ObservableCollection<BedrinthItem> Items { get; } = [];

    private void BedrinthPage_Loaded(object sender, EventArgs e)
    {
        Dispatcher.Dispatch(async () =>
        {
            Content = _activityIndicator;
            await LoadMore();
            Content = _BedrinthListView;
        });
    }

    private void ListView_Scrolled(object sender, ScrolledEventArgs e)
    {
        if (e.ScrollY >= _BedrinthListView.Height - _BedrinthListView.Bounds.Height)
        {
            Task.Run(LoadMore);
        }
    }

    private async Task LoadMore()
    {
        SearchPackagesResponse? response = await _bedrinthServicesProvider.SearchPackagesAsync(page: _pageIndex);

        if (response is null || _pageIndex > response.Data.TotalPages) return;

        _pageIndex++;
        foreach (PackageInfo package in response.Data.Items)
        {
            Items.Add(new BedrinthItem
            {
                Name = package.Name,
                Description = package.Description
            });
        }

        await LoadMore();
    }

    /* TODO Implement pagination
    private void BedrinthPage_Loaded(object sender, EventArgs e)
    {
        Dispatcher.Dispatch(async () =>
        {
            await FirstLoad();
        });
    }

    private void SetLoading(bool isLoading)
    {
        if (isLoading)
        {
            Content = _activityIndicator;
            _activityIndicator.IsRunning = true;
            _activityIndicator.IsVisible = true;
        }
        else
        {
            _activityIndicator.IsRunning = false;
            _activityIndicator.IsVisible = false;
            Content = _BedrinthListView;
        }
    }

    private async ValueTask FirstLoad()
    {
        SetLoading(true);

        SearchPackagesResponse? response = await _bedrinthServicesProvider.SearchPackagesAsync();
        totalPages = response?.Data.TotalPages ?? 1;



        SetLoading(false);
    }

    private async ValueTask Load(int pageIndex = 1)
    {
        SearchPackagesResponse? response = await _bedrinthServicesProvider.SearchPackagesAsync(page: _pageIndex);

        if (response is null || _pageIndex > response.Data.TotalPages) return;

        _pageIndex++;
        foreach (PackageInfo package in response.Data.Items)
        {
            Items.Add(new BedrinthItem
            {
                Name = package.Name,
                Description = package.Description
            });
        }

        await Load();
    }


    private const int MaxButtonsCount = 5;

    private Button _prev = new()
    {
        Text = "<",
        BackgroundColor = Colors.Transparent,
        HeightRequest = 32,
        WidthRequest = 32
    };

    private Button _next = new()
    {
        Text = ">",
        BackgroundColor = Colors.Transparent,
        HeightRequest = 32,
        WidthRequest = 32
    };

    private Button[] _buttons = new Button[MaxButtonsCount];

    private async ValueTask UpdateButtons(int totalPages)
    {

    }*/
}
