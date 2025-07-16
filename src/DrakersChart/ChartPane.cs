using System.Windows;
using System.Windows.Input;
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
    private readonly DateTimeAxisXGuideView axisXGuideView;
    private AxisXDrawRegion? currentXRegion = null;

    #region Draw Option

    private readonly SKPaint gridPaint = new()
    {
        Color = new SKColor(149, 149, 149),
        IsAntialias = false,
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 1
    };

    private Boolean reDrawAll;

    private Boolean useAxisXGuide = true;

    public Boolean UseAxisXGuide
    {
        get => this.useAxisXGuide;
        set
        {
            Boolean prev = this.useAxisXGuide;
            this.useAxisXGuide = value;
            if (prev == this.useAxisXGuide)
            {
                return;
            }

            RefreshChart();
        }
    }

    public Int32 XAxisGuideHeight => this.useAxisXGuide ? this.axisXGuideView.Height : 0;

    #endregion

    public SKColor BackgroundColor { get; set; } = SKColors.White;
    public Chart OwnerChart { get; set; }

    public ChartPane(Chart owner)
    {
        this.axisXGuideView = new DateTimeAxisXGuideView(owner.AxisXGridManager);
        this.OwnerChart = owner;
        this.PaintSurface += OnPaintSurface;
    }

    public void AddSeries(IChartSeries series)
    {
        this.seriesList.Add(series);
        this.OwnerChart.AxisXDataManager.AddData(series.GetAxisXValues());
        RefreshChart();
    }

    public void RefreshChart()
    {
        this.reDrawAll = true;
        InvalidateVisual();
    }

    public Boolean IsHoverOnAxisGuide()
    {
        if (this.UseAxisXGuide)
        {
            var pos = Mouse.GetPosition(this);
            return this.axisXGuideView.IsMouseHover(pos);
        }
        else
        {
            return false;
        }
    }

    public void UpdateMousePosition()
    {
        if (this.useAxisXGuide)
        {
            var pos = Mouse.GetPosition(this);
            this.currentXRegion = this.OwnerChart.AxisXDrawRegionManager.GetDrawRegion(pos.X);
            if (this.currentXRegion != null)
            {
                RefreshChart();
            }
        }
    }

    public void UpdateMouseLeave()
    {
        this.currentXRegion = null;
        RefreshChart();
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
            using var staticCanvas = new SKCanvas(this.staticLayer);
            DrawGrid(staticCanvas);
            DrawSeries(staticCanvas);
            DrawAxis(staticCanvas);
            this.reDrawAll = false;
        }

        canvas.DrawBitmap(this.staticLayer, 0, 0);
        DrawCrosshairAxisGuide(canvas);
        canvas.DrawRect(0.5f, 0.5f, this.staticLayer.Width - 3, this.staticLayer.Height - 3, Chart.BorderPaint);
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
        this.yScale.TargetMin = this.staticLayer.Height - (this.useAxisXGuide ? this.axisXGuideView.Height : 0);
        this.yScale.TargetMax = 0;
    }

    private void DrawGrid(SKCanvas canvas)
    {
        for (Int32 index = 1; index < this.OwnerChart.AxisXGridManager.Infos.Length; index++)
        {
            var eachInfo = this.OwnerChart.AxisXGridManager.Infos[index];
            canvas.DrawLine(eachInfo.Coordinate, 0, eachInfo.Coordinate, this.staticLayer.Height, this.gridPaint);
        }
    }

    private void DrawCrosshairAxisGuide(SKCanvas canvas)
    {
        if (this.currentXRegion != null)
        {
            var pos = Mouse.GetPosition(this);
            Int32 topY = this.staticLayer.Height - (this.useAxisXGuide ? this.axisXGuideView.Height : 0);
            this.axisXGuideView.DrawCurrentGuideValue(canvas, this.currentXRegion.Value, (Single)pos.X, topY, this.staticLayer.Width);
        }
    }

    private void DrawSeries(SKCanvas staticCanvas)
    {
        if (this.seriesList.Count == 0 || this.OwnerChart.AxisXDrawRegionManager.DrawRegionCount == 0)
        {
            return;
        }

        SetAxisYScale();
        foreach (var eachSeries in this.seriesList)
        {
            eachSeries.Draw(staticCanvas, this.yScale, this.OwnerChart.AxisXDrawRegionManager.DrawRegions);
        }
    }

    private void DrawAxis(SKCanvas staticCanvas)
    {
        Int32 topY = this.staticLayer.Height - (this.useAxisXGuide ? this.axisXGuideView.Height : 0);
        if (this.useAxisXGuide)
        {
            this.axisXGuideView.DrawView(staticCanvas, topY, this.staticLayer.Width, 0, 0);
        }
    }

    private void SetAxisYScale()
    {
        Single topMarginRatio = 0.01f;
        Single bottomMarginRatio = 0.01f;
        if (this.seriesList.Count > 0)
        {
            topMarginRatio = this.seriesList.Max(s => s.TopMarginRatio);
            bottomMarginRatio = this.seriesList.Max(s => s.BottomMarginRatio);
        }

        Int64[] axisXValues = this.OwnerChart.AxisXDrawRegionManager.DrawRegions.Select(r => r.X).ToArray();
        var ranges = this.seriesList
            .Select(s => s.GetAxisYRange(axisXValues))
            .ToArray();
        Double min = ranges.Min(r => r.Min);
        Double max = ranges.Min(r => r.Max);
        Double diff = max - min;
        this.yScale.SourceMin = min - (diff * bottomMarginRatio);
        this.yScale.SourceMax = max + (diff * topMarginRatio);
    }

    #endregion

    protected override Size MeasureOverride(Size availableSize)
    {
        this.reDrawAll = true;
        return base.MeasureOverride(availableSize);
    }
}