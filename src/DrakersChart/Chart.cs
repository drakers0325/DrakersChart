using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DrakersChart.Axis;

namespace DrakersChart;
public sealed class Chart : UserControl
{
    private readonly Grid mainGrid = new();
    private readonly Canvas canvas = new();
    private readonly List<ChartPane> chartList = [];
    private readonly List<Double> ratioList = [];
    private readonly CrosshairLayer crosshairLayer = new();
    private readonly SizeGripLayer gripLayer;

    internal AxisXDataManger AxisXDataManager { get; } = new();
    internal AxisXDrawRegionManager AxisXDrawRegionManager { get; }

    public ChartPane[] ChartPanes => this.chartList.ToArray();

    public Chart()
    {
        this.gripLayer = new SizeGripLayer(this, this.crosshairLayer);
        this.AxisXDrawRegionManager = new AxisXDrawRegionManager(this, this.AxisXDataManager);
        AddChild(this.mainGrid);
        this.mainGrid.Children.Add(this.canvas);
        this.mainGrid.Children.Add(this.crosshairLayer);
        this.mainGrid.Children.Add(this.gripLayer);
    }

    public void SetDisplayRange(Int32 startIndex, Int32 count)
    {
        this.AxisXDrawRegionManager.SetDisplayRange(startIndex, count);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        foreach (var eachChart in this.chartList)
        {
            eachChart.Width = availableSize.Width;
        }

        SetChartPaneHeight(availableSize.Height);
        this.AxisXDrawRegionManager.Width = availableSize.Width;
        this.AxisXDrawRegionManager.Height = availableSize.Height;
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
            sumHeight += this.chartList[index].Height;
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
        return newChart;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        var pos = e.GetPosition(this.crosshairLayer);
        this.crosshairLayer.UpdatePosition(pos);
        this.gripLayer.UpdatePosition(pos);
        base.OnMouseMove(e);
    }

    protected override void OnMouseLeave(MouseEventArgs e)
    {
        this.crosshairLayer.HideCrosshair();
        this.gripLayer.CancelSizeGrip();
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