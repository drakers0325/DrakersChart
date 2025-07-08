using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;
using Size = System.Windows.Size;

namespace DrakersChart;
public class ChartPane : SKGLElement
{
    private SKBitmap staticLayer = new();

    #region Draw Flag

    private Boolean reDrawAll;

    #endregion
    
    public SKColor BackgroundColor { get; set; } = SKColors.White;
    
    public ChartPane()
    {
        this.PaintSurface += OnPaintSurface;
    }

    #region Draw

    private void OnPaintSurface(Object? sender, SKPaintGLSurfaceEventArgs e)
    {
        
        DrawChart(e.Surface.Canvas);
    }
    
    private void DrawChart(SKCanvas canvas)
    {
        InitializeStaticLayer(canvas);
        canvas.Clear(this.BackgroundColor);
        
        canvas.DrawBitmap(this.staticLayer, 0, 0);
    }

    private void InitializeStaticLayer(SKCanvas canvas)
    {
        if (!this.reDrawAll)
        {
            return;
        }

        var bounds = canvas.LocalClipBounds;
        Int32 width  = (Int32)bounds.Width;
        Int32 height = (Int32)bounds.Height;
        this.staticLayer = new SKBitmap(width, height);
    }

    #endregion
    
    protected override Size MeasureOverride(Size availableSize)
    {
        this.reDrawAll = true;
        return base.MeasureOverride(availableSize);
    }
}