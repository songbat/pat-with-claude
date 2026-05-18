using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace AvaloniaClaudePet.Views;

public partial class PetWindow : Window
{
    private bool _isDragging;
    private Point _dragStart;

    public PetWindow()
    {
        InitializeComponent();
        Topmost = true;
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        var dragHandle = this.FindControl<Panel>("DragHandle");
        if (dragHandle != null)
        {
            dragHandle.PointerPressed += OnPointerPressed;
            dragHandle.PointerMoved += OnPointerMoved;
            dragHandle.PointerReleased += OnPointerReleased;
        }
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            _isDragging = true;
            _dragStart = e.GetPosition(this);
            e.Handled = true;
        }
    }

    private void OnPointerMoved(object? sender, PointerEventArgs e)
    {
        if (_isDragging && VisualRoot != null)
        {
            var currentPos = e.GetPosition(this);
            var delta = currentPos - _dragStart;
            var scaling = VisualRoot.RenderScaling;
            Position = new PixelPoint(
                Position.X + (int)(delta.X * scaling),
                Position.Y + (int)(delta.Y * scaling));
        }
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        _isDragging = false;
    }
}
