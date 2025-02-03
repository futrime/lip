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

    private void PackageDetails_Loaded(object sender, EventArgs e)
    {
        _avatar.Source = Item.Avatar;
        _name.Text = Item.Name;
        _description.Text = Item.Description;
        _author.Text = Item.Author;
        _hotness.Text = Item.Hotness.ToString();
        _updated.Text = Item.Updated;
        _projectUrl.Text = Item.ProjectUrl;

        foreach (string tag in Item.Tags)
            _tagsLayout.Children.Add(new TagView()
            {
                Text = tag
            });
    }
}
