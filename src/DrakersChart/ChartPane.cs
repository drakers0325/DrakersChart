using System.IO;
using Windows.ApplicationModel.UserDataTasks;
using DrakersChart.Axis;
using DrakersChart.Series;
using SkiaSharp;
using SkiaSharp.Views.Desktop;
using SkiaSharp.Views.WPF;
using Size = System.Windows.Size;

namespace DrakersChart;
public class ChartPane : SKGLElement
{
    private SKBitmap staticLayer = new();
    private readonly List<IChartSeries> seriesList = [];
    private readonly AxisYScale yScale = new();

    #region Draw Flag

    private Boolean reDrawAll;

    #endregion

    public SKColor BackgroundColor { get; set; } = SKColors.White;
    public Chart OwnerChart { get; set; }

    public ChartPane(Chart owner)
    {
        this.OwnerChart = owner;
        this.PaintSurface += OnPaintSurface;
    }

    public void AddSeries(IChartSeries series)
    {
        this.seriesList.Add(series);
        this.OwnerChart?.AxisXDataManager.AddData(series.GetAxisXValues());
        RefreshChart();
    }

    public void RefreshChart()
    {
        this.reDrawAll = true;
        InvalidateVisual();
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
        if (this.reDrawAll)
        {
            DrawSeries();
            this.reDrawAll = false;
        }

        canvas.DrawBitmap(this.staticLayer, 0, 0);
    }

    private void InitializeStaticLayer(SKCanvas canvas)
    {
        if (!this.reDrawAll)
        {
            return;
        }

        var bounds = canvas.LocalClipBounds;
        Int32 width = (Int32)bounds.Width;
        Int32 height = (Int32)bounds.Height;
        this.staticLayer = new SKBitmap(width, height);
        this.yScale.TargetMin = this.staticLayer.Height;
        this.yScale.TargetMax = 0;
    }

    private void DrawSeries()
    {
        if (this.seriesList.Count == 0)
        {
            return;
        }

        SetAxisYScale();
        using var staticCanvas = new SKCanvas(this.staticLayer);
        foreach (var eachSeries in this.seriesList)
        {
            eachSeries.Draw(staticCanvas, this.yScale, this.OwnerChart.AxisXDrawRegionManager.DrawRegions);
        }
        
        using var image = SKImage.FromBitmap(this.staticLayer);
        using var data  = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite("test.png");
        data.SaveTo(stream);
    }

    private void SetAxisYScale()
    {
        Double min = this.seriesList.Min(s => s.Min);
        Double minOffSet = Math.Abs(min * 0.1);
        Double max = this.seriesList.Max(s => s.Max);
        Double maxOffSet = Math.Abs(max * 0.1);
        this.yScale.SourceMin = min - minOffSet;
        this.yScale.SourceMax = max + maxOffSet;
    }

    #endregion

    protected override Size MeasureOverride(Size availableSize)
    {
        this.reDrawAll = true;
        return base.MeasureOverride(availableSize);
    }
}