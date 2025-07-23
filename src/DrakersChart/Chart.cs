using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using DrakersChart.Axis;
using SkiaSharp;

namespace DrakersChart;
public sealed class Chart : UserControl
{
    internal static readonly SKPaint BorderPaint = new()
    {
        Color = new SKColor(149, 149, 149),
        IsAntialias = false,
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 1
    };

    private readonly Grid mainGrid = new();
    private readonly Canvas canvas = new();
    private readonly List<ChartPane> chartList = [];
    private readonly List<Double> ratioList = [];
    private readonly CrosshairLayer crosshairLayer = new();
    private readonly SizeGripLayer gripLayer;
    private readonly Border chartBorder = new();
    private readonly ScrollBar axisXScrollBar = new();
    private readonly Slider displayCountSlider = new();

    internal AxisXDataManger AxisXDataManager { get; } = new();
    internal AxisXDrawRegionManager AxisXDrawRegionManager { get; }
    internal AxisXGridManager AxisXGridManager { get; }

    public Int32 LeftMargin
    {
        get => this.AxisXDrawRegionManager.LeftMargin;
        set => this.AxisXDrawRegionManager.LeftMargin = value;
    }

    public Int32 RightMargin
    {
        get => this.AxisXDrawRegionManager.RightMargin;
        set => this.AxisXDrawRegionManager.RightMargin = value;
    }

    public ChartPane[] ChartPanes => this.chartList.ToArray();

    private Boolean useLeftAxisYGuide;

    public Boolean UseLeftAxisYGuide
    {
        get => this.useLeftAxisYGuide;
        set
        {
            this.useLeftAxisYGuide = value;
            foreach (var eachPane in this.chartList)
            {
                eachPane.UseLeftAxisYGuide = this.useLeftAxisYGuide;
            }

            UpdateDrawRegion();
        }
    }

    private Boolean useRightAxisYGuide;

    public Boolean UseRightAxisYGuide
    {
        get => this.useRightAxisYGuide;
        set
        {
            this.useRightAxisYGuide = value;
            foreach (var eachPane in this.chartList)
            {
                eachPane.UseRightAxisYGuide = this.useRightAxisYGuide;
            }

            UpdateDrawRegion();
        }
    }

    public Boolean IsHoverOnAxisGuide { get; private set; }

    public Chart()
    {
        this.gripLayer = new SizeGripLayer(this, this.crosshairLayer);
        this.AxisXDrawRegionManager = new AxisXDrawRegionManager(this.AxisXDataManager);
        this.AxisXGridManager = new AxisXGridManager(this.AxisXDrawRegionManager);
        this.AxisXDataManager.DataUpdated += AxisXDataUpdated;
        
        InitializeMainGrid();
    }

    private void AxisXDataUpdated(Object? sender, EventArgs e)
    {
        Int32 largeChange = this.AxisXDataManager.DataCount / 100;
        this.displayCountSlider.Minimum = 1;
        this.displayCountSlider.Maximum = this.AxisXDataManager.DataCount;
        this.displayCountSlider.SmallChange = 1;
        this.displayCountSlider.TickFrequency = 1;
        this.displayCountSlider.IsSnapToTickEnabled = true;
        this.displayCountSlider.Focusable = false;
        this.displayCountSlider.LargeChange = largeChange;

        this.axisXScrollBar.Maximum = this.AxisXDataManager.DataCount - this.displayCountSlider.Value;
        this.axisXScrollBar.SmallChange = 1;
        this.axisXScrollBar.LargeChange = largeChange;
    }

    private void InitializeMainGrid()
    {
        AddChild(this.mainGrid);
        this.mainGrid.Children.Add(this.canvas);
        this.mainGrid.Children.Add(this.crosshairLayer);
        this.mainGrid.Children.Add(this.gripLayer);
        
        this.chartBorder.BorderThickness = new Thickness(1);
        this.chartBorder.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 149, 149, 149));
        this.mainGrid.Children.Add(this.chartBorder);

        InitializeScroll();
    }

    private void InitializeScroll()
    {
        this.axisXScrollBar.VerticalAlignment = VerticalAlignment.Bottom;
        this.axisXScrollBar.HorizontalAlignment = HorizontalAlignment.Left;
        this.axisXScrollBar.Orientation = Orientation.Horizontal;
        this.axisXScrollBar.Height = 18;
        this.axisXScrollBar.Margin = new Thickness(1, 0, 0, 1);

        this.mainGrid.Children.Add(this.axisXScrollBar);

        this.displayCountSlider.VerticalAlignment = VerticalAlignment.Bottom;
        this.displayCountSlider.HorizontalAlignment = HorizontalAlignment.Right;
        this.displayCountSlider.Orientation = Orientation.Horizontal;
        this.displayCountSlider.Width = 100;
        this.displayCountSlider.Margin = new Thickness(0, 0, 0, 1);

        this.mainGrid.Children.Add(this.displayCountSlider);
        
        this.axisXScrollBar.ValueChanged += AxisXScrollBarOnValueChanged;
        this.displayCountSlider.ValueChanged += DisplayCountSliderOnValueChanged;
    }

    #region Scroll
    
    private void AxisXScrollBarOnValueChanged(Object sender, RoutedPropertyChangedEventArgs<Double> e)
    {
        SetDisplayRange((Int32)this.axisXScrollBar.Value, (Int32)this.displayCountSlider.Value);
    }
    
    private void DisplayCountSliderOnValueChanged(Object sender, RoutedPropertyChangedEventArgs<Double> e)
    {
        this.axisXScrollBar.Maximum = this.AxisXDataManager.DataCount - (Int32)this.displayCountSlider.Value;
        this.axisXScrollBar.Value = this.AxisXDataManager.DataCount - (Int32)this.displayCountSlider.Value;
        
        SetDisplayRange((Int32)this.axisXScrollBar.Value, (Int32)this.displayCountSlider.Value);
    }

    #endregion


    public void SetDisplayRange(Int32 startIndex, Int32 count)
    {
        this.AxisXDrawRegionManager.SetDisplayRange(startIndex, count);
        this.axisXScrollBar.Value = startIndex;
        this.displayCountSlider.Value = count;
        RefreshChart();
    }

    private void RefreshChart()
    {
        foreach (var eachPane in this.chartList)
        {
            eachPane.RefreshChart();
        }
    }

    public void UpdateDrawRegion()
    {
        this.AxisXDrawRegionManager.Width = this.ActualWidth;
        if (this.chartList.Count == 0)
        {
            return;
        }

        Int32 leftWidth = this.chartList.Max(c => c.LeftAxisYGuideWidth);
        Int32 rightWidth = this.chartList.Max(c => c.RightAxisGuideWidth);
        this.AxisXDrawRegionManager.LeftAxisYGuideWidth = leftWidth;
        this.AxisXDrawRegionManager.RightAxisYGuideWidth = rightWidth;
        UpdateCrosshairMargin();

        Boolean useLeft = this.chartList.Any(c => c.UseLeftAxisYGuide);
        Boolean useRight = this.chartList.Any(c => c.UseRightAxisYGuide);
        foreach (var eachPane in this.chartList)
        {
            eachPane.UseLeftAxisYGuide = useLeft;
            eachPane.UseRightAxisYGuide = useRight;
        }
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        foreach (var eachChart in this.chartList)
        {
            eachChart.Width = availableSize.Width;
        }

        SetChartPaneHeight(availableSize.Height - this.axisXScrollBar.Height - 1);
        this.axisXScrollBar.Width = availableSize.Width - 100;
        this.AxisXDrawRegionManager.Width = availableSize.Width;
        this.gripLayer.UpdateGripArea();
        return base.MeasureOverride(availableSize);
    }

    /// <summary>
    /// 각 ChartPane의 높이를 비율대로 설정한다
    /// </summary>
    /// <param name="totalHeight">전체 Height</param>
    private void SetChartPaneHeight(Double totalHeight)
    {
        if (this.chartList.Count > 0 && this.ratioList.Count == 0)
        {
            this.ratioList.Clear();
            Double ratio = 1 / (Double)this.ratioList.Count;
            for (Int32 index = 0; index < this.chartList.Count; index++)
            {
                this.ratioList.Add(ratio);
            }
        }

        if (this.chartList.Count == 0)
        {
            return;
        }

        Double sumHeight = 0;
        for (Int32 index = 0; index < this.chartList.Count - 1; index++)
        {
            this.chartList[index].Height = (Int32)(totalHeight * this.ratioList[index]);
            Canvas.SetTop(this.chartList[index], sumHeight);
            sumHeight += this.chartList[index].Height - 1;
        }

        Canvas.SetTop(this.chartList[^1], sumHeight);
        this.chartList[^1].Height = totalHeight - sumHeight;
    }

    /// <summary>
    /// 현재 ChartPane높이에 따른 비율을 업데이트
    /// </summary>
    public void UpdateChartPaneRatio()
    {
        Double totalHeight = this.chartList.Sum(chart => chart.Height);
        this.ratioList.Clear();
        foreach (var eachPane in this.chartList)
        {
            this.ratioList.Add(eachPane.Height / totalHeight);
        }
    }

    /// <summary>
    /// 입력된 비율대로 각 ChartPane의 Height를 설정
    /// </summary>
    /// <param name="ratios">비율들</param>
    public void SetChartPaneHeightRatio(Double[] ratios)
    {
        if (ratios.Length == 0)
        {
            return;
        }

        if (this.chartList.Count != ratios.Length)
        {
            throw new ApplicationException("비율의 수는 ChartPane의 수와 같아야 합니다");
        }

        Double total = 0;
        for (Int32 index = 0; index < ratios.Length - 1; index++)
        {
            total += ratios[index];
        }

        if (total >= 1)
        {
            throw new ApplicationException("전체 비율이 1 이상 입니다");
        }

        this.ratioList.Clear();
        this.ratioList.AddRange(ratios);
        if (!Double.IsNaN(this.Height))
        {
            SetChartPaneHeight(this.Height);
        }
    }

    public ChartPane AddChart()
    {
        var newChart = new ChartPane(this);

        if (this.chartList.Count == 0)
        {
            this.ratioList.Add(1);
        }
        else
        {
            this.chartList[^1].UseAxisXGuide = false;
            Double totalRatio = this.ratioList.Sum();
            Double avg = totalRatio / this.ratioList.Count;
            Double ratio = totalRatio / (totalRatio + avg);

            for (Int32 index = 0; index < this.ratioList.Count; index++)
            {
                this.ratioList[index] *= ratio;
            }

            this.ratioList.Add(1 - this.ratioList.Sum());
        }

        Canvas.SetLeft(newChart, 0);
        Canvas.SetTop(newChart, 0);
        this.chartList.Add(newChart);
        this.canvas.Children.Add(newChart);
        UpdateCrosshairMargin();
        return newChart;
    }

    private void UpdateCrosshairMargin()
    {
        this.crosshairLayer.BottomMargin = this.chartList[^1].XAxisGuideHeight + (Int32)this.axisXScrollBar.Height;
        this.crosshairLayer.LeftMargin = this.AxisXDrawRegionManager.LeftAxisYGuideWidth;
        this.crosshairLayer.RightMargin = this.AxisXDrawRegionManager.RightAxisYGuideWidth;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        var pos = e.GetPosition(this.crosshairLayer);

        foreach (var eachPane in this.chartList)
        {
            eachPane.UpdateMousePosition();
        }

        if (this.chartList.Count > 0)
        {
            this.IsHoverOnAxisGuide = this.chartList.Any(c => c.IsMouseHoverOnAxis);
        }

        Boolean isHoverOnChartPane = this.chartList.Any(eachPane => eachPane.IsMouseHoverOnChartPane);

        if (this.IsHoverOnAxisGuide || !isHoverOnChartPane)
        {
            this.crosshairLayer.HideCrosshair();
        }
        else
        {
            this.crosshairLayer.UpdatePosition(pos);
        }

        this.gripLayer.UpdatePosition(pos);
        this.Cursor = this.gripLayer.IsInGripArea ? Cursors.SizeNS : Cursors.Arrow;
        base.OnMouseMove(e);
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        this.crosshairLayer.HideCrosshair();
        this.gripLayer.CancelSizeGrip();
        this.Cursor = Cursors.Arrow;
        foreach (var eachPane in this.chartList)
        {
            eachPane.UpdateMouseLeave();
        }

        base.OnMouseLeave(e);
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            this.gripLayer.LeftMouseDown();
        }

        base.OnMouseDown(e);
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Released)
        {
            this.gripLayer.LeftMouseUp();
        }

        base.OnMouseUp(e);
    }
}