using Lip.GUI.Pages.Bedrinth;
using Lip.GUI.Pages.Servers;

namespace Lip.GUI.Pages.PackageDetails;

public partial class PackageInstallationView : ContentView
{
    public PackageInstallationView()
    {
        InitializeComponent();
    }

    private void ContentView_Loaded(object sender, EventArgs e)
    {
        CreateViews(ServersPage.Current.ServerViews, ((PackageDetailsPage)BindingContext).Item);
    }

    private void CreateViews(IReadOnlyDictionary<string, ServerView> views, BedrinthItem item)
    {
        foreach (var view in views)
        {
            _itemsLayout.Children.Add(new PackageInstallationItemView(this, view.Value, item));
        }
    }
}
