using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace AvaloniaClaudePet.Controls;

public partial class PetControl : UserControl
{
    public static readonly StyledProperty<string> ExpressionProperty =
        AvaloniaProperty.Register<PetControl, string>(nameof(Expression), "idle");

    public static readonly StyledProperty<string> StatusTextProperty =
        AvaloniaProperty.Register<PetControl, string>(nameof(StatusText), "");

    public string Expression
    {
        get => GetValue(ExpressionProperty);
        set => SetValue(ExpressionProperty, value);
    }

    public string StatusText
    {
        get => GetValue(StatusTextProperty);
        set => SetValue(StatusTextProperty, value);
    }

    private readonly Dictionary<string, Color> _bodyColors = new()
    {
        ["idle"] = Color.FromRgb(180, 180, 180),
        ["thinking"] = Color.FromRgb(107, 142, 35),
        ["working"] = Color.FromRgb(65, 105, 225),
        ["error"] = Color.FromRgb(205, 92, 92),
        ["success"] = Color.FromRgb(50, 205, 50),
        ["waiting"] = Color.FromRgb(255, 215, 0),
    };

    public PetControl()
    {
        InitializeComponent();
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        // Cat is drawn on the left side
        var catCx = 64.0;
        var catCy = Bounds.Height / 2 + 8;
        var expression = Expression;
        var color = _bodyColors.GetValueOrDefault(expression, _bodyColors["idle"]);

        DrawCat(context, catCx, catCy, color, expression);
        DrawBubble(context, catCx, catCy);
    }

    private void DrawCat(DrawingContext context, double cx, double cy, Color color, string expression)
    {
        var headR = 40;

        // Cat head
        context.DrawEllipse(new SolidColorBrush(color), null, new Rect(cx - headR, cy - headR, headR * 2, headR * 2));

        // Ears
        var earBrush = new SolidColorBrush(Color.FromRgb((byte)(color.R * 0.8), (byte)(color.G * 0.8), (byte)(color.B * 0.8)));
        context.DrawGeometry(earBrush, null, CreateTriangle(cx - 30, cy - 25, cx - 12, cy - 50, cx - 5, cy - 28));
        context.DrawGeometry(earBrush, null, CreateTriangle(cx + 30, cy - 25, cx + 12, cy - 50, cx + 5, cy - 28));

        // Inner ears
        var pinkBrush = new SolidColorBrush(Color.FromArgb(100, 255, 182, 193));
        context.DrawGeometry(pinkBrush, null, CreateTriangle(cx - 26, cy - 27, cx - 14, cy - 44, cx - 8, cy - 29));
        context.DrawGeometry(pinkBrush, null, CreateTriangle(cx + 26, cy - 27, cx + 14, cy - 44, cx + 8, cy - 29));

        DrawExpression(context, expression, cx, cy);
    }

    private void DrawBubble(DrawingContext context, double catCx, double catCy)
    {
        var status = StatusText;
        if (string.IsNullOrEmpty(status)) return;

        // Measure text
        var fontSize = 11.0;
        var typeface = new Typeface("Inter");
        var formatted = new FormattedText(status, System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight, typeface, fontSize, new SolidColorBrush(Color.FromRgb(51, 51, 51)));

        var textWidth = formatted.Width;
        var textHeight = formatted.Height;

        // Bubble dimensions
        var padX = 10.0;
        var padY = 6.0;
        var bubbleW = textWidth + padX * 2;
        var bubbleH = textHeight + padY * 2;

        // Position: top-right of cat head
        var bubbleX = catCx + 38;
        var bubbleY = catCy - 48 - bubbleH;
        if (bubbleY < 2) bubbleY = 2;

        // Tail (small triangle pointing left-down toward cat)
        var tailTipX = bubbleX - 4;
        var tailTipY = bubbleY + bubbleH + 8;
        var tail1X = bubbleX + 8;
        var tail1Y = bubbleY + bubbleH - 2;
        var tail2X = bubbleX;
        var tail2Y = bubbleY + bubbleH + 12;
        context.DrawGeometry(new SolidColorBrush(Color.FromArgb(230, 255, 255, 255)), null,
            CreateTriangle(tailTipX, tailTipY, tail1X, tail1Y, tail2X, tail2Y));

        // Bubble body (rounded rect)
        var cornerR = 10.0;
        var rect = new Rect(bubbleX, bubbleY, bubbleW, bubbleH);
        context.DrawGeometry(
            new SolidColorBrush(Color.FromArgb(230, 255, 255, 255)),
            new Pen(new SolidColorBrush(Color.FromArgb(80, 0, 0, 0)), 1),
            CreateRoundedRect(rect, cornerR));

        // Shadow
        var shadowRect = new Rect(bubbleX + 2, bubbleY + 2, bubbleW, bubbleH);
        context.DrawGeometry(
            new SolidColorBrush(Color.FromArgb(20, 0, 0, 0)),
            null,
            CreateRoundedRect(shadowRect, cornerR));

        // Re-draw bubble on top of shadow
        context.DrawGeometry(
            new SolidColorBrush(Color.FromArgb(230, 255, 255, 255)),
            new Pen(new SolidColorBrush(Color.FromArgb(80, 0, 0, 0)), 1),
            CreateRoundedRect(rect, cornerR));

        // Re-draw tail
        context.DrawGeometry(new SolidColorBrush(Color.FromArgb(230, 255, 255, 255)),
            new Pen(new SolidColorBrush(Color.FromArgb(80, 0, 0, 0)), 1),
            CreateTriangle(tailTipX, tailTipY, tail1X, tail1Y, tail2X, tail2Y));

        // Text
        var textX = bubbleX + padX;
        var textY = bubbleY + padY;
        context.DrawText(formatted, new Point(textX, textY));
    }

    private static Geometry CreateRoundedRect(Rect rect, double radius)
    {
        var geo = new StreamGeometry();
        using var ctx = geo.Open();
        ctx.BeginFigure(new Point(rect.Left + radius, rect.Top), true);
        ctx.LineTo(new Point(rect.Right - radius, rect.Top));
        ctx.ArcTo(new Point(rect.Right, rect.Top + radius), new Size(radius, radius), 0, false, SweepDirection.Clockwise);
        ctx.LineTo(new Point(rect.Right, rect.Bottom - radius));
        ctx.ArcTo(new Point(rect.Right - radius, rect.Bottom), new Size(radius, radius), 0, false, SweepDirection.Clockwise);
        ctx.LineTo(new Point(rect.Left + radius, rect.Bottom));
        ctx.ArcTo(new Point(rect.Left, rect.Bottom - radius), new Size(radius, radius), 0, false, SweepDirection.Clockwise);
        ctx.LineTo(new Point(rect.Left, rect.Top + radius));
        ctx.ArcTo(new Point(rect.Left + radius, rect.Top), new Size(radius, radius), 0, false, SweepDirection.Clockwise);
        ctx.EndFigure(true);
        return geo;
    }

    private void DrawExpression(DrawingContext context, string expression, double cx, double cy)
    {
        var black = new SolidColorBrush(Colors.Black);
        var white = new SolidColorBrush(Colors.White);

        switch (expression)
        {
            case "idle":
                DrawEyes(context, cx, cy, black, white, false);
                DrawMouth(context, cx, cy + 12, 10, false);
                break;
            case "thinking":
                DrawEyes(context, cx, cy, black, white, false);
                DrawDot(context, cx + 22, cy - 42, 3, black);
                DrawDot(context, cx + 30, cy - 50, 2, black);
                DrawDot(context, cx + 36, cy - 55, 1.5, black);
                DrawDot(context, cx, cy + 14, 3, black);
                break;
            case "working":
                DrawEyes(context, cx, cy, black, white, false);
                context.DrawLine(new Pen(black, 2), new Point(cx - 20, cy - 14), new Point(cx - 8, cy - 16));
                context.DrawLine(new Pen(black, 2), new Point(cx + 20, cy - 14), new Point(cx + 8, cy - 16));
                context.DrawLine(new Pen(black, 2), new Point(cx - 8, cy + 14), new Point(cx + 8, cy + 14));
                break;
            case "error":
                var xe = 5;
                context.DrawLine(new Pen(black, 2), new Point(cx - 14 - xe, cy - 5 - xe), new Point(cx - 14 + xe, cy - 5 + xe));
                context.DrawLine(new Pen(black, 2), new Point(cx - 14 + xe, cy - 5 - xe), new Point(cx - 14 - xe, cy - 5 + xe));
                context.DrawLine(new Pen(black, 2), new Point(cx + 14 - xe, cy - 5 - xe), new Point(cx + 14 + xe, cy - 5 + xe));
                context.DrawLine(new Pen(black, 2), new Point(cx + 14 + xe, cy - 5 - xe), new Point(cx + 14 - xe, cy - 5 + xe));
                DrawMouth(context, cx, cy + 18, 10, true);
                break;
            case "success":
                context.DrawLine(new Pen(black, 2), new Point(cx - 19, cy - 3), new Point(cx - 14, cy - 8));
                context.DrawLine(new Pen(black, 2), new Point(cx - 9, cy - 3), new Point(cx - 14, cy - 8));
                context.DrawLine(new Pen(black, 2), new Point(cx + 9, cy - 3), new Point(cx + 14, cy - 8));
                context.DrawLine(new Pen(black, 2), new Point(cx + 19, cy - 3), new Point(cx + 14, cy - 8));
                DrawMouth(context, cx, cy + 10, 14, false);
                var blush = new SolidColorBrush(Color.FromArgb(60, 255, 150, 150));
                context.DrawEllipse(blush, null, new Rect(cx - 28, cy + 2, 12, 8));
                context.DrawEllipse(blush, null, new Rect(cx + 16, cy + 2, 12, 8));
                break;
            case "waiting":
                DrawEyes(context, cx, cy, black, white, true);
                context.DrawEllipse(null, new Pen(black, 2), new Rect(cx - 5, cy + 10, 10, 10));
                break;
        }
    }

    private void DrawEyes(DrawingContext context, double cx, double cy, ISolidColorBrush black, ISolidColorBrush white, bool big)
    {
        var eyeR = big ? 9 : 7;
        var pupilR = big ? 5 : 4;
        var highlightR = big ? 3 : 2;

        foreach (var ox in new[] { -14, 14 })
        {
            context.DrawEllipse(white, null, new Rect(cx + ox - eyeR, cy - 5 - eyeR, eyeR * 2, eyeR * 2));
            context.DrawEllipse(black, null, new Rect(cx + ox - pupilR, cy - 5 - pupilR, pupilR * 2, pupilR * 2));
            context.DrawEllipse(white, null, new Rect(cx + ox + 1 - highlightR, cy - 8 - highlightR, highlightR * 2, highlightR * 2));
        }
    }

    private void DrawMouth(DrawingContext context, double cx, double cy, double width, bool sad)
    {
        var half = width / 2;
        var stream = new StreamGeometry();
        using (var ctx = stream.Open())
        {
            ctx.BeginFigure(new Point(cx - half, cy), false);
            ctx.QuadraticBezierTo(new Point(cx, sad ? cy - 8 : cy + 8), new Point(cx + half, cy));
        }
        context.DrawGeometry(null, new Pen(new SolidColorBrush(Colors.Black), 2), stream);
    }

    private void DrawDot(DrawingContext context, double cx, double cy, double r, ISolidColorBrush brush)
    {
        context.DrawEllipse(brush, null, new Rect(cx - r, cy - r, r * 2, r * 2));
    }

    private static PathGeometry CreateTriangle(double x1, double y1, double x2, double y2, double x3, double y3)
    {
        var geo = new PathGeometry();
        var fig = new PathFigure { StartPoint = new Point(x1, y1), IsClosed = true };
        fig.Segments.Add(new LineSegment { Point = new Point(x2, y2) });
        fig.Segments.Add(new LineSegment { Point = new Point(x3, y3) });
        geo.Figures.Add(fig);
        return geo;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ExpressionProperty)
        {
            Opacity = 0.3;
            InvalidateVisual();
            Dispatcher.UIThread.Post(() => Opacity = 1.0);
        }
        else if (change.Property == StatusTextProperty)
        {
            InvalidateVisual();
        }
    }
}
