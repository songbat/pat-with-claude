using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;

namespace AvaloniaClaudePet.Controls;

public partial class PetControl : UserControl
{
    public static readonly StyledProperty<string> ExpressionProperty =
        AvaloniaProperty.Register<PetControl, string>(nameof(Expression), "idle");

    public string Expression
    {
        get => GetValue(ExpressionProperty);
        set => SetValue(ExpressionProperty, value);
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

        var expression = Expression;
        var color = _bodyColors.GetValueOrDefault(expression, _bodyColors["idle"]);
        var cx = Bounds.Width / 2;
        var cy = Bounds.Height / 2 + 8;
        var headR = 40;

        // Cat head
        var headBrush = new SolidColorBrush(color);
        context.DrawEllipse(headBrush, null, new Rect(cx - headR, cy - headR, headR * 2, headR * 2));

        // Ears (triangles)
        var earBrush = new SolidColorBrush(Color.FromRgb(
            (byte)(color.R * 0.8),
            (byte)(color.G * 0.8),
            (byte)(color.B * 0.8)));

        // Left ear
        context.DrawGeometry(earBrush, null, CreateTriangle(cx - 30, cy - 25, cx - 12, cy - 50, cx - 5, cy - 28));
        // Right ear
        context.DrawGeometry(earBrush, null, CreateTriangle(cx + 30, cy - 25, cx + 12, cy - 50, cx + 5, cy - 28));

        // Inner ears (pink)
        var pinkBrush = new SolidColorBrush(Color.FromArgb(100, 255, 182, 193));
        context.DrawGeometry(pinkBrush, null, CreateTriangle(cx - 26, cy - 27, cx - 14, cy - 44, cx - 8, cy - 29));
        context.DrawGeometry(pinkBrush, null, CreateTriangle(cx + 26, cy - 27, cx + 14, cy - 44, cx + 8, cy - 29));

        // Expression-specific features
        DrawExpression(context, expression, cx, cy);
    }

    private void DrawExpression(DrawingContext context, string expression, double cx, double cy)
    {
        var black = new SolidColorBrush(Colors.Black);
        var white = new SolidColorBrush(Colors.White);

        switch (expression)
        {
            case "idle":
                // Normal eyes
                DrawEyes(context, cx, cy, black, white, false);
                // Small smile
                DrawMouth(context, cx, cy + 12, 10, false);
                break;

            case "thinking":
                // Half-closed eyes (looking up-right)
                DrawEyes(context, cx, cy, black, white, false);
                // Thought dots
                DrawDot(context, cx + 22, cy - 42, 3, black);
                DrawDot(context, cx + 30, cy - 50, 2, black);
                DrawDot(context, cx + 36, cy - 55, 1.5, black);
                // Hmm mouth
                DrawDot(context, cx, cy + 14, 3, black);
                break;

            case "working":
                // Determined eyes (focused)
                DrawEyes(context, cx, cy, black, white, false);
                // Slight concentration brow
                context.DrawLine(new Pen(black, 2), new Point(cx - 20, cy - 14), new Point(cx - 8, cy - 16));
                context.DrawLine(new Pen(black, 2), new Point(cx + 20, cy - 14), new Point(cx + 8, cy - 16));
                // Straight mouth
                context.DrawLine(new Pen(black, 2), new Point(cx - 8, cy + 14), new Point(cx + 8, cy + 14));
                break;

            case "error":
                // X eyes
                var xe = 5;
                context.DrawLine(new Pen(black, 2), new Point(cx - 14 - xe, cy - 5 - xe), new Point(cx - 14 + xe, cy - 5 + xe));
                context.DrawLine(new Pen(black, 2), new Point(cx - 14 + xe, cy - 5 - xe), new Point(cx - 14 - xe, cy - 5 + xe));
                context.DrawLine(new Pen(black, 2), new Point(cx + 14 - xe, cy - 5 - xe), new Point(cx + 14 + xe, cy - 5 + xe));
                context.DrawLine(new Pen(black, 2), new Point(cx + 14 + xe, cy - 5 - xe), new Point(cx + 14 - xe, cy - 5 + xe));
                // Sad mouth
                DrawMouth(context, cx, cy + 18, 10, true);
                break;

            case "success":
                // Happy eyes (^_^)
                context.DrawLine(new Pen(black, 2), new Point(cx - 19, cy - 3), new Point(cx - 14, cy - 8));
                context.DrawLine(new Pen(black, 2), new Point(cx - 9, cy - 3), new Point(cx - 14, cy - 8));
                context.DrawLine(new Pen(black, 2), new Point(cx + 9, cy - 3), new Point(cx + 14, cy - 8));
                context.DrawLine(new Pen(black, 2), new Point(cx + 19, cy - 3), new Point(cx + 14, cy - 8));
                // Big smile
                DrawMouth(context, cx, cy + 10, 14, false);
                // Blush
                var blush = new SolidColorBrush(Color.FromArgb(60, 255, 150, 150));
                context.DrawEllipse(blush, null, new Rect(cx - 28, cy + 2, 12, 8));
                context.DrawEllipse(blush, null, new Rect(cx + 16, cy + 2, 12, 8));
                break;

            case "waiting":
                // Big pleading eyes
                DrawEyes(context, cx, cy, black, white, true);
                // Open mouth (small O)
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
            // White
            context.DrawEllipse(white, null, new Rect(cx + ox - eyeR, cy - 5 - eyeR, eyeR * 2, eyeR * 2));
            // Pupil
            context.DrawEllipse(black, null, new Rect(cx + ox - pupilR, cy - 5 - pupilR, pupilR * 2, pupilR * 2));
            // Highlight
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
            ctx.QuadraticBezierTo(
                new Point(cx, sad ? cy - 8 : cy + 8),
                new Point(cx + half, cy));
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
            InvalidateVisual();
        }
    }
}
