namespace Lip.GUI.Controls;

public partial class InfoBarPage : ContentPage
{
    public static new readonly BindableProperty ContentProperty = BindableProperty.Create(
        nameof(Content),
        typeof(View),
        typeof(ContentPage),
        null,
        propertyChanged: (bindable, oldValue, newValue) =>
        {
            var page = (InfoBarPage)bindable;
            page._infoBarContentView.Content = (View)newValue;
        });

    public new View Content
    {
        get => (View)GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    private readonly ContentView _infoBarContentView = new();
    private readonly InfoBar _infoBar = new();

    public InfoBarPage()
    {
        var @base = ((ContentPage)this);
        @base.Content = new Grid()
        {
            Children =
            {
                _infoBarContentView,
                new VerticalStackLayout()
                {
                    Children = { _infoBar }
                }
            }
        };
    }

    
}
