namespace Lip.GUI.Pages.PackageDetails;

public static class ThemeColorGenerator
{
    public static (Color Background, Color Text) GenerateColors(int seed, AppTheme theme)
    {
        var _random = new Random(seed);

        // 生成随机HSL基础值
        double hue = _random.NextDouble();
        double saturation = theme == AppTheme.Light
            ? _random.NextDouble() * 0.3 + 0.1  // 浅色主题：10%-40% 
            : _random.NextDouble() * 0.4 + 0.4; // 深色主题：40%-80%

        double baseLightness = theme == AppTheme.Light
            ? _random.NextDouble() * 0.2 + 0.7  // 浅色主题：70%-90%
            : _random.NextDouble() * 0.2 + 0.2; // 深色主题：20%-40%

        // 生成背景色
        var bgColor = Color.FromHsla(hue, saturation, baseLightness);

        // 生成协调的文本颜色
        Color textColor = GenerateContrastColor(bgColor, theme);

        return (bgColor, textColor);
    }

    private static Color GenerateContrastColor(Color baseColor, AppTheme theme)
    {
        // 保持相同色相，调整亮度和饱和度
        baseColor.ToHsl(out var h, out var s, out var l);

        // 计算对比色亮度差
        double contrastLightness = theme == AppTheme.Light
            ? l - 0.5  // 浅色主题使用更暗的文本
            : l + 0.5; // 深色主题使用更亮的文本

        // 约束亮度范围并降低饱和度
        contrastLightness = Math.Clamp(contrastLightness, 0.1, 0.9);
        double contrastSaturation = Math.Clamp(s * 0.7, 0.2, 0.8);

        return Color.FromHsla(h, contrastSaturation, contrastLightness);
    }
}

public partial class TagView : ContentView
{
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text),
        typeof(string),
        typeof(TagView),
        default(string),
        propertyChanged: OnTextChanged);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public TagView()
    {
        InitializeComponent();

        // 初始颜色设置
        UpdateColors();

        // 监听主题变化
        Application.Current!.RequestedThemeChanged += (_, _) => UpdateColors();
    }

    private void UpdateColors()
    {
        (Color bgColor, Color textColor) = ThemeColorGenerator.GenerateColors(
            _text.Text?.GetHashCode() ?? 0,
            Application.Current!.RequestedTheme);

        _border.BackgroundColor = bgColor;
        _border.Stroke = textColor;
        _text.TextColor = textColor;
    }

    private static void OnTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = ((TagView)bindable);
        view._text.Text = (string)newValue;
        view.UpdateColors();
    }

    private void Button_Clicked(object sender, EventArgs e)
    {

    }
}
