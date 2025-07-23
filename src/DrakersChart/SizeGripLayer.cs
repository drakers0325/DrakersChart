using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DrakersChart;
internal class SizeGripLayer : Canvas
{
    private readonly struct GripInfo(Rect gripArea, Double topLimit, Double bottomLimit, ChartPane topPane, ChartPane bottomPane)
    {
        public Rect GripArea { get; } = gripArea;
        public Double TopLimit { get; } = topLimit;
        public Double BottomLimit { get; } = bottomLimit;
        public ChartPane TopPane { get; } = topPane;
        public ChartPane BottomPane { get; } = bottomPane;
    }

    private readonly Chart chart;
    private readonly CrosshairLayer crosshairLayer;
    private readonly List<GripInfo> gripInfoList = [];

    private GripInfo? currentInfo;
    private Point pos;
    private Boolean sizeChangeOn;
    private Double gripMargin = 2.5;
    private Double gripHeight = 5;
    
    public Brush LineBrush { get; set; } = Brushes.Black;
    public Double LineThickness { get; set; } = 1;

    public Double GripHeight
    {
        get => this.gripHeight;
        set
        {
            this.gripHeight = value;
            this.gripMargin = this.gripHeight / 2;
            UpdateGripArea();
        }
    }

    public Boolean AllowSizeGrip { get; set; } = true;

    public Boolean IsInGripArea { get; private set; }

    public SizeGripLayer(Chart chart, CrosshairLayer crosshairLayer)
    {
        this.chart = chart;
        this.crosshairLayer = crosshairLayer;
        this.Background = Brushes.Transparent;
        this.SnapsToDevicePixels = true;
        this.UseLayoutRounding = true;
        this.IsHitTestVisible = false;
        RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
    }

    /// <summary>
    /// ChartPane의 높이 조절 Grip의 구역을 업데이트
    /// </summary>
    public void UpdateGripArea()
    {
        this.gripInfoList.Clear();
        
        Double y = 0;
        for (Int32 index = 0; index < this.chart.ChartPanes.Length - 1; index++)
        {
            y += this.chart.ChartPanes[index].Height;
            var eachPane = this.chart.ChartPanes[index];
            var nextPane = this.chart.ChartPanes[index + 1];
            Double eachTop = GetTop(eachPane);
            Double nextBottom = eachTop + eachPane.Height + nextPane.Height;

            var area = new Rect(0, y - this.gripMargin, eachPane.Width, this.GripHeight);
            Double topLimit = eachTop + 20;
            Double bottomLimit = nextBottom - 20;
            this.gripInfoList.Add(new GripInfo(area, topLimit, bottomLimit, eachPane, nextPane));
        }
    }

    /// <summary>
    /// 현재 마우스 위치 업데이트
    /// </summary>
    /// <param name="position">위치</param>
    public void UpdatePosition(Point position)
    {
        this.pos = position;
        CheckSizeGrip();
        if (this.sizeChangeOn)
        {
            InvalidateVisual();
        }
    }

    /// <summary>
    /// Grip 취소
    /// </summary>
    public void CancelSizeGrip()
    {
        this.Cursor = Cursors.Arrow;
        this.sizeChangeOn = false;
        this.crosshairLayer.AllowCrosshair = true;
        this.currentInfo = null;
        InvalidateVisual();
    }

    public void LeftMouseDown()
    {
        this.sizeChangeOn = this.AllowSizeGrip && this.IsInGripArea;
        if (this.sizeChangeOn)
        {
            this.crosshairLayer.AllowCrosshair = false;
            this.crosshairLayer.InvalidateVisual();
        }

        InvalidateVisual();
    }

    public void LeftMouseUp()
    {
        Boolean prevOn = this.sizeChangeOn;
        this.sizeChangeOn = false;
        if (prevOn && this.currentInfo != null)
        {
            var info = this.currentInfo.Value;
            Double totalHeight = info.TopPane.Height + info.BottomPane.Height;
            Double topPaneY = GetTop(info.TopPane);
            Int32 y = (Int32)this.pos.Y;
            Double topHeight = (Int32)(y - topPaneY);
            Double bottomHeight = totalHeight - topHeight;

            info.TopPane.Height = topHeight;
            info.BottomPane.Height = bottomHeight;
            SetTop(info.BottomPane, y - 1);
            UpdateGripArea();
            this.chart.UpdateChartPaneRatio();
        }

        this.crosshairLayer.AllowCrosshair = true;
        this.currentInfo = null;
        InvalidateVisual();
    }

    /// <summary>
    /// 현재 마우스의 위치가 Grip 영역 내에 있는지 확인하고 각종 Flag들 설정과 Cursor 모양 변경
    /// </summary>
    private void CheckSizeGrip()
    {
        if (!this.AllowSizeGrip || this.sizeChangeOn)
        {
            return;
        }

        this.IsInGripArea = false;
        foreach (var eachInfo in this.gripInfoList)
        {
            this.IsInGripArea = !eachInfo.GripArea.IsEmpty && eachInfo.GripArea.Contains(this.pos.X, this.pos.Y);
            if (!this.IsInGripArea)
            {
                continue;
            }

            this.currentInfo = eachInfo;
            break;
        }

        this.Cursor = this.IsInGripArea ? Cursors.SizeNS : Cursors.Arrow;
    }

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        if (!this.sizeChangeOn)
        {
            return;
        }

        var pen = new Pen(this.LineBrush, this.LineThickness);
        pen.Freeze();

        //사이즈 변경 Limit 넘기지 않도록 설정
        Double y = this.pos.Y;
        if (this.currentInfo != null)
        {
            if (this.currentInfo.Value.TopLimit >= y)
            {
                y = this.currentInfo.Value.TopLimit;
            }

            if (this.currentInfo.Value.BottomLimit <= y)
            {
                y = this.currentInfo.Value.BottomLimit;
            }
        }

        dc.PushGuidelineSet(
            new GuidelineSet(
                [0.5],
                [this.pos.Y + 0.5]));
        dc.DrawLine(pen, new Point(0, y), new Point(this.ActualWidth, y));

        dc.Pop();
    }
}