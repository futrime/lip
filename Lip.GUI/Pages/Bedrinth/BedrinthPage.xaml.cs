using System.Collections.ObjectModel;
using Bedrinth;
using Lip.GUI.Pages.PackageDetails;
using static Bedrinth.PackageInfo;

namespace Lip.GUI.Pages.Bedrinth;

public class BedrinthItem
{
    public required string Identifier { get; set; }

    public required string Avatar { get; set; }

    public required string Name { get; set; }

    public required string Description { get; set; }

    public required string Author { get; set; }

    public required double Hotness { get; set; }

    public required string Updated { get; set; }

    public required string ProjectUrl { get; set; }

    public required IEnumerable<string> Tags { get; set; }

    public required IEnumerable<string> Versions { get; set; }

    public required PackageInfo Original { get; set; }

    public static implicit operator BedrinthItem(PackageInfo package) => new()
    {
        Identifier = package.Identifier,
        Avatar = package.AvatarUrl,
        Name = package.Name,
        Description = package.Description,
        Author = package.Author,
        Hotness = package.Hotness,
        Updated = package.Updated,
        ProjectUrl = package.ProjectUrl,
        Tags = package.Tags,
        Versions = from version in package.Versions ?? [] select version.Version,
        Original = package,
    };
}

public partial class BedrinthPage : ContentPage
{
    public BedrinthPage()
    {
        InitializeComponent();

        Current = this;
    }

    public static BedrinthPage? Current { get; private set; }

    public BedrinthServicesProvider BedrinthServicesProvider { get; } = new();

    private int _pageIndex = 1;

    private readonly ActivityIndicator _activityIndicator = new() { IsRunning = true };

    public ObservableCollection<BedrinthItem> Items { get; } = [];

    private void BedrinthPage_Loaded(object sender, EventArgs e)
    {
        Dispatcher.Dispatch(async () =>
        {
            //Content = _activityIndicator;
            await LoadMore();
            //Content = _bedrinthListView;
        });
    }

    private async Task LoadMore()
    {
        SearchPackagesResponse? response = await BedrinthServicesProvider.SearchPackagesAsync(page: _pageIndex);

        if (response is null || _pageIndex > response.Data.TotalPages) return;

        _pageIndex++;
        foreach (PackageInfo package in response.Data.Items)
        {
            Items.Add(package);
        }

        await LoadMore();
    }

    private void BedrinthListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
    {
        var item = e.SelectedItem as BedrinthItem;

        if (item is null) return;

        Navigation.PushAsync(new PackageDetailsPage(item));
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
