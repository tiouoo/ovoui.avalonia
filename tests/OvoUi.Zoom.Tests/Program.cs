using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using OvoUi;
using OvoUi.Controls;
using OvoUi.SmoothScroll;
using OvoUi.SmoothScroll.Controls;
using Minimap = OvoUi.Controls.ZoomMinimap;
using SmoothMinimap = OvoUi.SmoothScroll.Controls.ZoomMinimap;

AppBuilder.Configure<Application>().UseSkia().UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false }).SetupWithoutStarting();
Application.Current!.Styles.Add(new OvoUiTheme());
Application.Current.Styles.Add(new OvoScrollViewTheme());

void Check(bool condition, string message)
{
    if (!condition) throw new Exception(message);
    Console.WriteLine("PASS: " + message);
}
void Layout(Window window)
{
    window.UpdateLayout();
    AvaloniaHeadlessPlatform.ForceRenderTimerTick(5);
    Dispatcher.UIThread.RunJobs();
    window.UpdateLayout();
}
Point At(Control control, double x, double y, Window window) => control.TranslatePoint(new Point(x, y), window)!.Value;
void Click(Window window, Point point)
{
    window.MouseDown(point, MouseButton.Left);
    window.MouseUp(point, MouseButton.Left);
    Layout(window);
}

void Drag(Window target, MouseButton button)
{
    target.MouseDown(new Point(250, 150), button);
    Layout(target);
    target.MouseMove(new Point(290, 170));
    for (var i = 0; i < 8; i++) { Layout(target); Thread.Sleep(10); }
    target.MouseUp(new Point(290, 170), button);
    Layout(target);
}

void VerifyPositions(Control minimap, Action<Avalonia.Layout.HorizontalAlignment, Avalonia.Layout.VerticalAlignment> setPosition,
    Action<Vector> setOffset)
{
    foreach (var horizontal in new[] { Avalonia.Layout.HorizontalAlignment.Left, Avalonia.Layout.HorizontalAlignment.Center, Avalonia.Layout.HorizontalAlignment.Right })
    foreach (var vertical in new[] { Avalonia.Layout.VerticalAlignment.Top, Avalonia.Layout.VerticalAlignment.Bottom })
    {
        setPosition(horizontal, vertical);
        setOffset(default);
        var target = (Window)TopLevel.GetTopLevel(minimap)!;
        Layout(target);
        var original = minimap.TranslatePoint(default, target)!.Value;
        setOffset(new Vector(12, -16));
        Layout(target);
        var shifted = minimap.TranslatePoint(default, target)!.Value;
        Check(shifted - original == new Vector(12, -16), $"{minimap.GetType().Namespace}: {vertical} {horizontal} supports offsets");
    }
}

Grid CreateCanvas()
{
    var grid = new Grid { Width = 1600, Height = 1000, ColumnDefinitions = new ColumnDefinitions("*,*"), RowDefinitions = new RowDefinitions("*,*") };
    var labels = new[] { "Zoom", "Pan", "Overview", "Explore" };
    var colors = new[] { "#6695E8", "#61BDA6", "#DCAE68", "#BC85C8" };
    for (var i = 0; i < 4; i++)
    {
        var border = new Border
        {
            Background = new SolidColorBrush(Color.Parse(colors[i])),
            Child = new TextBlock { Text = labels[i], FontSize = 64, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center }
        };
        Grid.SetColumn(border, i % 2);
        Grid.SetRow(border, i / 2);
        grid.Children.Add(border);
    }
    return grid;
}
void Snapshot(Window target, string name)
{
    if (args.Length == 0) return;
    Directory.CreateDirectory(args[0]);
    Layout(target);
    using var frame = target.GetLastRenderedFrame();
    using var output = File.Create(Path.Combine(args[0], name));
    frame?.Save(output, PngBitmapEncoderOptions.Default);
}
var zoom = new ZoomView { Content = CreateCanvas() };
var window = new Window { Width = 800, Height = 500, Content = zoom };
window.Show();
Layout(window);
var map = zoom.GetVisualDescendants().OfType<Minimap>().Single();
Snapshot(window, "zoom-view.png");
Check(((Control)zoom.Content!).Bounds.Position == default, "ZoomView content remains at the coordinate origin");
Check(map.Bounds.Width == 180 && map.Bounds.Height == 120, "ZoomView minimap is measured and placed");
zoom.Zoom = 1;
Layout(window);
var before = zoom.ViewportRect;
Click(window, At(map, 160, 95, window));
Check(zoom.ViewportRect.X > before.X && zoom.ViewportRect.Y > before.Y, "ZoomView click navigates both axes");
zoom.IsMinimapVisible = false;
Layout(window);
Check(!map.IsVisible, "ZoomView minimap hides");
zoom.Zoom = 2;
zoom.IsMinimapVisible = true;
Layout(window);
Check(map.IsVisible && Math.Abs(map.ViewportRect.Width - 400) < 1, "ZoomView minimap restores with current zoom");
zoom.IsMinimapNavigationEnabled = false;
before = zoom.ViewportRect;
Click(window, At(map, 20, 20, window));
Check(zoom.ViewportRect == before, "ZoomView navigation can be disabled");
zoom.IsMinimapNavigationEnabled = true;
zoom.IsMinimapDragEnabled = true;
var start = At(map, 50, 40, window);
window.MouseDown(start, MouseButton.Left);
before = zoom.ViewportRect;
window.MouseMove(At(map, 100, 65, window));
window.MouseUp(At(map, 100, 65, window), MouseButton.Left);
Layout(window);
Check(zoom.ViewportRect.X > before.X, "ZoomView drag navigates");
zoom.IsMinimapDragEnabled = false;
window.MouseDown(start, MouseButton.Left);
before = zoom.ViewportRect;
window.MouseMove(At(map, 100, 65, window));
window.MouseUp(At(map, 100, 65, window), MouseButton.Left);
Check(zoom.ViewportRect == before, "ZoomView drag can be disabled independently of click");
zoom.IsMinimapDragEnabled = true;
window.MouseDown(At(map, 100, 65, window), MouseButton.Left);
zoom.IsMinimapVisible = false;
Layout(window);
zoom.IsMinimapVisible = true;
Layout(window);
before = zoom.ViewportRect;
window.MouseMove(At(map, 140, 85, window));
window.MouseUp(At(map, 140, 85, window), MouseButton.Left);
Check(zoom.ViewportRect == before, "Hiding a minimap cancels active pointer capture");
window.MouseDown(At(map, 100, 65, window), MouseButton.Left);
window.MouseMove(new Point(2000, 2000));
window.MouseUp(new Point(2000, 2000), MouseButton.Left);
Check(zoom.ViewportRect.Right <= zoom.ContentSize.Width && zoom.ViewportRect.Bottom <= zoom.ContentSize.Height,
    "ZoomView navigation stays inside content bounds");
var buttons = zoom.GetVisualDescendants().OfType<Button>().Where(b => b.Content is string text && new[] { "−", "+", "Fit all", "Fill" }.Contains(text)).ToArray();
Check(buttons.Length == 0, "ZoomView default template contains no zoom toolbar");
var factor = zoom.Zoom;
zoom.ZoomIn();
Check(zoom.Zoom > factor, "External callers can zoom in");
zoom.ZoomOut();
Check(Math.Abs(zoom.Zoom - factor) < 0.0001, "External callers can zoom out");
window.Width = 900;
Layout(window);
zoom.FitToAll();
Check(zoom.ContentSize.Width * zoom.Zoom <= zoom.Bounds.Width && zoom.ContentSize.Height * zoom.Zoom <= zoom.Bounds.Height,
    "External Fit shows all content");
zoom.FitToContent();
Check(zoom.ContentSize.Width * zoom.Zoom >= zoom.Bounds.Width && zoom.ContentSize.Height * zoom.Zoom >= zoom.Bounds.Height,
    "External Fill covers the viewport");
window.Width = 800;
Layout(window);
zoom.Zoom = 2;
factor = zoom.Zoom;
window.MouseWheel(new Point(250, 150), new Vector(0, 1));
Check(zoom.Zoom == factor, "Ctrl-wheel mode leaves unmodified wheel available for panning");
window.MouseWheel(new Point(250, 150), new Vector(0, 1), RawInputModifiers.Control);
Check(zoom.Zoom > factor, "ZoomView supports Ctrl-wheel zoom");
zoom.WheelZoomModifiers = KeyModifiers.None;
factor = zoom.Zoom;
window.MouseWheel(new Point(250, 150), new Vector(0, 1));
Check(zoom.Zoom > factor, "ZoomView supports wheel-only zoom");
zoom.IsZoomEnabled = false;
factor = zoom.Zoom;
window.MouseWheel(new Point(250, 150), new Vector(0, 1));
Check(zoom.Zoom == factor, "Disabling zoom also disables wheel zoom");
zoom.IsZoomEnabled = true;
zoom.ShowScrollBars = false;
Check(zoom.GetVisualDescendants().OfType<ScrollBar>().All(b => !b.IsVisible), "Scrollbars can be hidden");
zoom.PanButton = MouseButton.Left;
before = zoom.ViewportRect;
Drag(window, MouseButton.Left);
Check(zoom.ViewportRect.X < before.X, "ZoomView supports left-button panning");
zoom.PanButton = MouseButton.Right;
before = zoom.ViewportRect;
Drag(window, MouseButton.Left);
Check(zoom.ViewportRect == before, "ZoomView ignores unselected pan button");
Drag(window, MouseButton.Right);
Check(zoom.ViewportRect.X < before.X, "ZoomView supports right-button panning");
VerifyPositions(map, (h, v) => { zoom.MinimapHorizontalAlignment = h; zoom.MinimapVerticalAlignment = v; },
    value => zoom.MinimapOffset = value);
before = zoom.ViewportRect;
Click(window, At(map, 100, 65, window));
Check(zoom.ViewportRect != before, "Offset ZoomView minimap remains clickable");
zoom.Content = new Border { Width = 2000, Height = 800 };
Layout(window);
Check(zoom.ContentSize == new Size(2000, 800), "Replacing ZoomView content updates overview extent");
zoom.Content = new TextBlock { Text = "Small natural content" };
Layout(window);
Check(((Control)zoom.Content).Bounds.Size == zoom.ContentSize, "Small ZoomView content keeps its natural size");

var scroll = new OvoScrollView
{
    Content = CreateCanvas(),
    IsZoomEnabled = true, IsMinimapVisible = true,
    ContentOrientation = ScrollContentOrientation.Both,
    HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
    VerticalScrollBarVisibility = ScrollBarVisibility.Hidden
};
window.Content = scroll;
Layout(window);
var smoothMap = scroll.GetVisualDescendants().OfType<SmoothMinimap>().Single();
Snapshot(window, "smooth-zooming.png");
Check(smoothMap.Bounds.Width == 180 && smoothMap.IsVisible, "SmoothScroll minimap is visible");
Click(window, At(smoothMap, 160, 95, window));
Check(scroll.Offset.X > 0 && scroll.Offset.Y > 0, "SmoothScroll minimap click moves content");
scroll.ZoomTo(2, false);
for (var i = 0; i < 30 && scroll.ZoomFactor != 2; i++) { Layout(window); Thread.Sleep(10); }
Layout(window);

Check(Math.Abs(smoothMap.ContentSize.Width - 1600) < 1 && Math.Abs(smoothMap.ViewportRect.Width - 400) < 1,
    "SmoothScroll minimap uses unscaled coordinates at 200%");
scroll.IsMinimapVisible = false;
Layout(window);
scroll.Offset = new Vector(100, 200);
scroll.IsMinimapVisible = true;
Layout(window);
Check(smoothMap.IsVisible && Math.Abs(smoothMap.ViewportRect.X - 50) < 1, "SmoothScroll hide/show refreshes current position");
window.MouseDown(At(smoothMap, 40, 40, window), MouseButton.Left);
var dragOffset = scroll.Offset;
window.MouseMove(At(smoothMap, 95, 65, window));
window.MouseUp(At(smoothMap, 95, 65, window), MouseButton.Left);
Layout(window);
Check(scroll.Offset.X > dragOffset.X && scroll.Offset.Y > dragOffset.Y, "SmoothScroll minimap drag navigates at 200%");
scroll.IsMinimapDragEnabled = false;
window.MouseDown(At(smoothMap, 40, 40, window), MouseButton.Left);
dragOffset = scroll.Offset;
window.MouseMove(At(smoothMap, 95, 65, window));
window.MouseUp(At(smoothMap, 95, 65, window), MouseButton.Left);
Check(scroll.Offset == dragOffset, "SmoothScroll drag can be disabled independently of click");
scroll.MinimapWidth = 240;
scroll.MinimapHorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left;
scroll.MinimapVerticalAlignment = Avalonia.Layout.VerticalAlignment.Top;
Layout(window);
Check(smoothMap.Bounds.Width == 240 && smoothMap.Bounds.X == 8 && smoothMap.Bounds.Y == 8,
    "Minimap size and position update through properties");
scroll.IsMinimapNavigationEnabled = false;
var offset = scroll.Offset;
Click(window, At(smoothMap, 20, 20, window));
Check(scroll.Offset == offset, "SmoothScroll minimap navigation can be disabled");
scroll.PanButton = MouseButton.Left;
scroll.Offset = new Vector(800, 600);
for (var i = 0; i < 8; i++) { Layout(window); Thread.Sleep(10); }
offset = scroll.Offset;
Drag(window, MouseButton.Left);
Check(scroll.Offset.X < offset.X, "SmoothScroll supports left-button panning");
scroll.PanButton = MouseButton.Right;
offset = scroll.Offset;
Drag(window, MouseButton.Left);
Check(scroll.Offset == offset, "SmoothScroll ignores unselected pan button");
Drag(window, MouseButton.Right);
Check(scroll.Offset.X < offset.X, "SmoothScroll supports right-button panning");
VerifyPositions(smoothMap, (h, v) => { scroll.MinimapHorizontalAlignment = h; scroll.MinimapVerticalAlignment = v; },
    value => scroll.MinimapOffset = value);
scroll.IsMinimapNavigationEnabled = true;
offset = scroll.Offset;
Click(window, At(smoothMap, 100, 65, window));
Check(scroll.Offset != offset, "Offset SmoothScroll minimap remains clickable");
scroll.WheelZoomModifiers = KeyModifiers.Control;
factor = scroll.ZoomFactor;
window.MouseWheel(new Point(250, 150), new Vector(0, 1));
for (var i = 0; i < 30; i++) { Layout(window); Thread.Sleep(10); }
Check(scroll.ZoomFactor == factor, "SmoothScroll Ctrl-wheel mode does not zoom on plain wheel");
window.MouseWheel(new Point(250, 150), new Vector(0, 0.25), RawInputModifiers.Control);
for (var i = 0; i < 40; i++) { Layout(window); Thread.Sleep(10); }
Check(scroll.ZoomFactor > factor, "SmoothScroll supports Ctrl-wheel including fractional deltas");
scroll.WheelZoomModifiers = KeyModifiers.None;
factor = scroll.ZoomFactor;
window.MouseWheel(new Point(250, 150), new Vector(0, 1));
for (var i = 0; i < 40; i++) { Layout(window); Thread.Sleep(10); }
Check(scroll.ZoomFactor > factor, "SmoothScroll supports wheel-only zoom");
window.Close();
Console.WriteLine("All zoom navigation checks passed.");








