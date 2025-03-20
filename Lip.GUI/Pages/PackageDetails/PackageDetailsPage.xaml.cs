using Bedrinth;
using Lip.GUI.Pages.Bedrinth;

namespace Lip.GUI.Pages.PackageDetails;

public partial class PackageDetailsPage
    : ContentPage
{
    public BedrinthItem Item { get; set; }

    public IEnumerable<ServerInfo> Servers => MauiProgram.Config.Servers;

    public PackageDetailsPage(BedrinthItem item)
    {
        Item = item;
        InitializeComponent();
    }

    private async void PackageDetails_Loaded(object sender, EventArgs e)
    {
        GetPackageResponse? temp = await BedrinthPage.Current!.BedrinthServicesProvider.GetPackageAsync(Item.Identifier) ??
            throw new Exception("Package not found");

        Item = temp.Data;

        _avatar.Source = Item.Avatar;
        _name.Text = Item.Name;
        _description.Text = Item.Description;
        _author.Text = Item.Author;
        _hotness.Text = Item.Hotness.ToString();
        _updated.Text = Item.Updated;
        _projectUrl.Text = Item.ProjectUrl;

        _installationView.Item = Item;

        foreach (string tag in Item.Tags)
            _tagsLayout.Children.Add(new TagView() { Text = tag });
    }
}
