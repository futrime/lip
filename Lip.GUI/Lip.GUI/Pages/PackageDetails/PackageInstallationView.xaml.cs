using Lip.GUI.Pages.Bedrinth;
using Lip.GUI.Pages.Servers;

namespace Lip.GUI.Pages.PackageDetails;

public partial class PackageInstallationView : ContentView
{
    public PackageInstallationView()
    {
        InitializeComponent();
    }

    public BedrinthItem? Item
    {
        get => field;
        set
        {
            field = value;

            Dispatcher.Dispatch(() =>
            {
                if (Item is null) return;

                _picker.ItemsSource = Item.Versions.ToList();
                _picker.SelectedIndex = 0;

                CreateViews(ServersPage.Current.ServerViews, Item);
            });
        }
    }

    private List<PackageInstallationItemView> _installationItemViews = [];

    private void CreateViews(IReadOnlyDictionary<string, ServerView> views, BedrinthItem item)
    {
        _itemsLayout.Children.Clear();

        foreach (KeyValuePair<string, ServerView> view in views)
        {
            var installationItemView = new PackageInstallationItemView(this, view.Value, item);
            _installationItemViews.Add(installationItemView);
            _itemsLayout.Children.Add(installationItemView);
        }
    }

    public void ItemViewCheckBoxChanged(PackageInstallationItemView sender, EventArgs args)
    {
        _button.IsEnabled = true;
    }

    private void ApplyButton_Clicked(object sender, EventArgs e)
    {

    }
}
