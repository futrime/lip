namespace Lip.GUI;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());

        window.Created += Window_Created;
        window.Stopped += Window_Stopped;
        window.Destroying += Window_Destroying;

        return window;
    }


    private void Window_Created(object? sender, EventArgs e)
    {
    }


    private void Window_Stopped(object? sender, EventArgs e)
    {
        MauiProgram.SaveConfig();
    }

    private void Window_Destroying(object? sender, EventArgs e)
    {
        MauiProgram.SaveConfig();
    }

}
