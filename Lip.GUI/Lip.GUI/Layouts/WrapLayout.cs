using Microsoft.Maui.Layouts;

namespace Lip.GUI.Layouts;

public partial class WrapLayout : Layout
{
    public static readonly BindableProperty OrientationProperty =
        BindableProperty.Create(
            nameof(Orientation),
            typeof(StackOrientation),
            typeof(WrapLayout),
            StackOrientation.Vertical,
            BindingMode.TwoWay,
            propertyChanged: (bindable, oldvalue, newvalue) => ((WrapLayout)bindable).InvalidateMeasure());

    public StackOrientation Orientation
    {
        get { return (StackOrientation)GetValue(OrientationProperty); }
        set { SetValue(OrientationProperty, value); }
    }

    public static readonly BindableProperty SpacingProperty =
        BindableProperty.Create(
            nameof(Spacing),
            typeof(double),
            typeof(WrapLayout),
            default(double),
            BindingMode.TwoWay,
            propertyChanged: (bindable, oldvalue, newvalue) => ((WrapLayout)bindable).InvalidateMeasure());

    public double Spacing
    {
        get { return (double)GetValue(SpacingProperty); }
        set { SetValue(SpacingProperty, value); }
    }

    protected override ILayoutManager CreateLayoutManager()
    {
        return new WrapLayoutManager(this);
    }
}

public class WrapLayoutManager(WrapLayout layout) : ILayoutManager
{
    private readonly WrapLayout _layout = layout;

    public Size Measure(double widthConstraint, double heightConstraint)
    {
        if (_layout.Orientation is StackOrientation.Vertical)
        {
            return MeasureVertical(widthConstraint, heightConstraint);
        }
        else
        {
            return MeasureHorizontal(widthConstraint, heightConstraint);
        }
    }
    public Size ArrangeChildren(Rect bounds)
    {
        if (_layout.Orientation == StackOrientation.Vertical)
        {
            return VerticalLayout(bounds);
        }
        else
        {
            return HorizontalLayout(bounds);
        }
    }

    private Size MeasureVertical(double widthConstraint, double heightConstraint)
    {
        int columnCount = 1;
        double width = 0;
        double height = 0;
        double minWidth = 0;
        double minHeight = 0;
        double heightUsed = 0;

        foreach (IView? item in _layout.Children)
        {
            Size size = item.Measure(widthConstraint, heightConstraint);
            width = Math.Max(width, size.Width);

            double newHeight = height + size.Height + _layout.Spacing;
            if (newHeight > heightConstraint)
            {
                columnCount++;
                heightUsed = Math.Max(height, heightUsed);
                height = size.Height;
            }
            else
                height = newHeight;

            minHeight = Math.Max(minHeight, size.Height);
            minWidth = Math.Max(minWidth, size.Width);
        }

        if (columnCount > 1)
        {
            height = Math.Max(height, heightUsed);
            width *= columnCount;
        }

        return new Size(width, height);
    }

    private Size MeasureHorizontal(double widthConstraint, double heightConstraint)
    {
        int rowCount = 1;
        double width = 0;
        double height = 0;
        double minWidth = 0;
        double minHeight = 0;
        double widthUsed = 0;

        foreach (IView? item in _layout.Children)
        {
            Size size = item.Measure(widthConstraint, heightConstraint);
            height = Math.Max(height, size.Height);

            var newWidth = width + size.Width + _layout.Spacing;
            if (newWidth > widthConstraint)
            {
                rowCount++;
                widthUsed = Math.Max(width, widthUsed);
                width = size.Width;
            }
            else
                width = newWidth;

            minHeight = Math.Max(minHeight, size.Height);
            minWidth = Math.Max(minWidth, size.Width);
        }

        if (rowCount > 1)
        {
            width = Math.Max(width, widthUsed);
            height = (height + _layout.Spacing) * rowCount - _layout.Spacing;
        }

        return new Size(width, height);
    }

    private Size VerticalLayout(Rect bounds)
    {
        double x = bounds.X;
        double y = bounds.Y;
        double colWidth = 0;
        double maxHeight = 0;

        foreach (IView? child in _layout.Children)
        {
            Size size = child.Measure(bounds.Width, bounds.Height);

            if (y + size.Height + _layout.Spacing > bounds.Height)
            {
                y = bounds.Y;
                x += colWidth + _layout.Spacing;
                colWidth = 0;
            }

            var region = new Rect(x, y, size.Width, size.Height);
            child.Arrange(region);
            y += size.Height + _layout.Spacing;
            colWidth = Math.Max(colWidth, size.Width);
            maxHeight = Math.Max(maxHeight, y);
        }

        return new Size(x + colWidth, maxHeight);
    }

    private Size HorizontalLayout(Rect bounds)
    {
        double x = bounds.X;
        double y = bounds.Y;
        double rowHeight = 0;
        double maxWidth = 0;

        foreach (IView? child in _layout.Children)
        {
            Size size = child.Measure(bounds.Width, bounds.Height);

            if (x + size.Width + _layout.Spacing > bounds.Width)
            {
                x = bounds.X;
                y += rowHeight + _layout.Spacing;
                rowHeight = 0;
            }

            var region = new Rect(x, y, size.Width, size.Height);
            child.Arrange(region);
            x += size.Width + _layout.Spacing;
            rowHeight = Math.Max(rowHeight, size.Height);
            maxWidth = Math.Max(maxWidth, x);
        }

        return new Size(maxWidth, y + rowHeight);
    }
}
