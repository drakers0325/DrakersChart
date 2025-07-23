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
    private readonly DateTimeAxisXGuideView axisXGuideView;
    private readonly AxisYGuideView leftYGuideView;
    private readonly AxisYGridManager leftYGridManager;
    private readonly AxisYScale leftYScale = new();
    private readonly AxisYGuideView rightYGuideView;
    private readonly AxisYGridManager rightYGridManager;
    private readonly AxisYScale rightYScale = new();
    private readonly List<IChartSeries> seriesList = [];
    private AxisXDrawRegion? currentXRegion;
    private SKBitmap staticLayer = new();

    public SKColor BackgroundColor { get; set; } = SKColors.White;
    public Chart OwnerChart { get; set; }

    public Boolean IsMouseHoverOnChartPane { get; private set; }
    public Boolean IsMouseHoverOnAxis { get; private set; }

    public ChartPane(Chart owner)
    {
        this.axisXGuideView = new DateTimeAxisXGuideView(owner.AxisXGridManager);
        this.leftYGuideView = new AxisYGuideView
        {
            UseGuideView = false,
            Location = AxisYGuideLocation.Left
        };
        this.leftYGridManager = new AxisYGridManager(this.leftYScale);
        this.rightYGuideView = new AxisYGuideView
        {
            UseGuideView = false,
            Location = AxisYGuideLocation.Right
        };
        this.rightYGridManager = new AxisYGridManager(this.rightYScale);
        this.OwnerChart = owner;
        this.PaintSurface += OnPaintSurface;
    }

    public void AddSeries(IChartSeries series)
    {
        this.seriesList.Add(series);
        this.OwnerChart.AxisXDataManager.AddData(series.GetAxisXValues());
        this.leftYGuideView.UseGuideView = false;
        this.rightYGuideView.UseGuideView = false;
        foreach (var eachSeries in this.seriesList)
        {
            switch (eachSeries.AxisYGuideLocation)
            {
                case AxisYGuideLocation.Left:
                    this.leftYGuideView.UseGuideView = true;
                    break;
                case AxisYGuideLocation.Right:
                    this.rightYGuideView.UseGuideView = true;
                    break;
            }
        }

        this.OwnerChart.UpdateDrawRegion();
        RefreshChart();
    }

    public void RefreshChart()
    {
        this.reDrawAll = true;
        InvalidateVisual();
    }

    public void UpdateMousePosition()
    {
        Boolean prevHover = this.IsMouseHoverOnChartPane;

        var pos = Mouse.GetPosition(this);
        this.IsMouseHoverOnAxis = IsHoverOnAxisGuide();
        this.IsMouseHoverOnChartPane = pos.X >= 0 && pos.X <= this.ActualWidth &&
                                       pos.Y >= 0 && pos.Y <= this.ActualHeight;

        Boolean updateVisual = false;
        if (this.useAxisXGuide)
        {
            this.currentXRegion = this.OwnerChart.AxisXDrawRegionManager.GetDrawRegion(pos.X);
            if (this.currentXRegion != null)
            {
                updateVisual = true;
            }
        }

        if (prevHover != this.IsMouseHoverOnChartPane || (this.IsMouseHoverOnChartPane && !this.IsMouseHoverOnAxis))
        {
            updateVisual = true;
        }

        if (updateVisual)
        {
            InvalidateVisual();
        }
    }

    public void UpdateMouseLeave()
    {
        this.currentXRegion = null;
        this.IsMouseHoverOnAxis = false;
        this.IsMouseHoverOnChartPane = false;
        InvalidateVisual();
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        this.reDrawAll = true;
        return base.MeasureOverride(availableSize);
    }

    private Boolean IsHoverOnAxisGuide()
    {
        if (!this.UseAxisXGuide && !this.UseLeftAxisYGuide && !this.UseRightAxisYGuide)
        {
            return false;
        }

        var pos = Mouse.GetPosition(this);
        return this.axisXGuideView.IsMouseHover(pos) ||
               this.leftYGuideView.IsMouseHover(pos) ||
               this.rightYGuideView.IsMouseHover(pos);
    }

    #region Draw Option

    private readonly SKPaint gridPaint = new()
    {
        Color = new SKColor(149, 149, 149, 70),
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

    public Boolean UseLeftAxisYGuide
    {
        get => this.leftYGuideView.UseGuideView;
        set
        {
            Boolean prev = this.leftYGuideView.UseGuideView;
            this.leftYGuideView.UseGuideView = value;
            if (prev == this.leftYGuideView.UseGuideView)
            {
                return;
            }

            RefreshChart();
        }
    }

    public Boolean UseRightAxisYGuide
    {
        get => this.rightYGuideView.UseGuideView;
        set
        {
            Boolean prev = this.rightYGuideView.UseGuideView;
            this.rightYGuideView.UseGuideView = value;
            if (prev == this.rightYGuideView.UseGuideView)
            {
                return;
            }

            RefreshChart();
        }
    }

    public Int32 LeftAxisYGuideWidth => this.leftYGuideView.UseGuideView ? this.leftYGuideView.Width : 0;
    public Int32 RightAxisGuideWidth => this.rightYGuideView.UseGuideView ? this.rightYGuideView.Width : 0;

    public Int32 XAxisGuideHeight => this.useAxisXGuide ? this.axisXGuideView.Height : 0;

    #endregion

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
            SetScales();
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
        this.rightYScale.TargetMin = this.leftYScale.TargetMin = this.staticLayer.Height - (this.useAxisXGuide ? this.axisXGuideView.Height : 0);
        this.rightYScale.TargetMax = this.leftYScale.TargetMax = 0;
    }

    private void SetScales()
    {
        if (this.seriesList.Count == 0 || this.OwnerChart.AxisXDrawRegionManager.DrawRegionCount == 0)
        {
            return;
        }

        var rightSeries = this.seriesList.Where(s => s.AxisYGuideLocation == AxisYGuideLocation.Right).ToArray();
        var leftSeries = this.seriesList.Where(s => s.AxisYGuideLocation == AxisYGuideLocation.Left).ToArray();
        SetAxisYScale(rightSeries.Length == 0 ? this.seriesList.ToArray() : rightSeries, this.rightYScale);
        SetAxisYScale(leftSeries.Length == 0 ? this.seriesList.ToArray() : leftSeries, this.leftYScale);
    }

    private void DrawGrid(SKCanvas canvas)
    {
        DrawVerticalGrid(canvas);
        DrawHorizontalGrid(canvas);
    }

    private void DrawVerticalGrid(SKCanvas canvas)
    {
        for (Int32 index = 1; index < this.OwnerChart.AxisXGridManager.Infos.Length; index++)
        {
            var eachInfo = this.OwnerChart.AxisXGridManager.Infos[index];
            canvas.DrawLine(eachInfo.Coordinate, 0, eachInfo.Coordinate, this.staticLayer.Height, this.gridPaint);
        }
    }

    private void DrawHorizontalGrid(SKCanvas canvas)
    {
        Single leftMargin = this.UseLeftAxisYGuide ? this.leftYGuideView.Width : 0;
        Single rightMargin = this.UseRightAxisYGuide ? this.rightYGuideView.Width : 0;

        if (this.UseLeftAxisYGuide)
        {
            this.leftYGridManager.Update();
            foreach (var eachLine in this.leftYGridManager.GuideLines)
            {
                canvas.DrawLine(leftMargin, eachLine.Location, this.staticLayer.Width - rightMargin, eachLine.Location, this.gridPaint);
            }
        }

        if (this.UseRightAxisYGuide)
        {
            this.rightYGridManager.Update();
            foreach (var eachLine in this.rightYGridManager.GuideLines)
            {
                canvas.DrawLine(leftMargin, eachLine.Location, this.staticLayer.Width - rightMargin, eachLine.Location, this.gridPaint);
            }
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

        if (this.IsMouseHoverOnChartPane && !this.IsMouseHoverOnAxis)
        {
            var pos = Mouse.GetPosition(this);
            if (this.UseLeftAxisYGuide)
            {
                this.leftYGuideView.DrawCurrentGuideValue(canvas, this.leftYScale, pos.Y, this.staticLayer.Width);
            }

            if (this.UseRightAxisYGuide)
            {
                this.rightYGuideView.DrawCurrentGuideValue(canvas, this.rightYScale, pos.Y, this.staticLayer.Width);
            }
        }
    }

    private void DrawSeries(SKCanvas staticCanvas)
    {
        if (this.seriesList.Count == 0 || this.OwnerChart.AxisXDrawRegionManager.DrawRegionCount == 0)
        {
            return;
        }

        foreach (var eachSeries in this.seriesList)
        {
            var scale = eachSeries.AxisYGuideLocation == AxisYGuideLocation.Right ? this.rightYScale : this.leftYScale;
            eachSeries.Draw(staticCanvas, scale, this.OwnerChart.AxisXDrawRegionManager.DrawRegions);
        }
    }

    private void DrawAxis(SKCanvas staticCanvas)
    {
        Int32 topY = this.staticLayer.Height - (this.useAxisXGuide ? this.axisXGuideView.Height : 0);
        if (this.useAxisXGuide)
        {
            Single leftGuideWidth = this.leftYGuideView.UseGuideView ? this.leftYGuideView.Width : 0;
            Single rightGuideWidth = this.rightYGuideView.UseGuideView ? this.rightYGuideView.Width : 0;
            this.axisXGuideView.DrawView(staticCanvas, topY, this.staticLayer.Width, leftGuideWidth, rightGuideWidth);
        }

        Single bottomMargin = this.useAxisXGuide ? this.axisXGuideView.Height : 0;
        if (this.UseLeftAxisYGuide)
        {
            this.leftYGuideView.DrawView(
                staticCanvas,
                this.leftYScale,
                this.leftYGridManager,
                this.staticLayer.Width,
                this.staticLayer.Height,
                bottomMargin);
        }

        if (this.UseRightAxisYGuide)
        {
            this.rightYGuideView.DrawView(
                staticCanvas,
                this.rightYScale,
                this.rightYGridManager,
                this.staticLayer.Width,
                this.staticLayer.Height,
                bottomMargin);
        }
    }

    private void SetAxisYScale(IChartSeries[] series, AxisYScale yScale)
    {
        Single topMarginRatio;
        Single bottomMarginRatio;
        if (series.Length > 0)
        {
            topMarginRatio = series.Max(s => s.TopMarginRatio);
            bottomMarginRatio = series.Max(s => s.BottomMarginRatio);
        }
        else
        {
            return;
        }

        Int64[] axisXValues = this.OwnerChart.AxisXDrawRegionManager.DrawRegions.Select(r => r.X).ToArray();
        var ranges = series
            .Select(s => s.GetAxisYRange(axisXValues))
            .ToArray();
        Double min = ranges.Min(r => r.Min);
        Double max = ranges.Max(r => r.Max);
        Double diff = max - min;
        yScale.SourceMin = min - diff * bottomMarginRatio;
        yScale.SourceMax = max + diff * topMarginRatio;
    }

    #endregion
}