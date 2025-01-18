using Fonts;

namespace Lip.GUI;

public partial class AppShell : Shell
{
    public AppShell()
    {

        InitializeComponent();
    }

    protected override async void OnNavigated(ShellNavigatedEventArgs args)
    {
        base.OnNavigated(args);

        Page currentPage = CurrentPage;
        await currentPage.FadeTo(0, 0);
        await currentPage.FadeTo(1, 250);
    }
}
