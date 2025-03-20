using Lip.GUI.Pages.PackageDetails;

namespace Lip.GUI;

public partial class AppShell : Shell
{
    public AppShell()
    {
        this.
        InitializeComponent();

        Routing.RegisterRoute(nameof(PackageDetailsPage), typeof(PackageDetailsPage));
    }

    //protected override async void OnNavigated(ShellNavigatedEventArgs args)
    //{
    //    await CurrentPage.FadeTo(0, 150);

    //    base.OnNavigated(args);
    //    Current.Opacity = 0;

    //    await CurrentPage.FadeTo(1, 150);
    //}
}
