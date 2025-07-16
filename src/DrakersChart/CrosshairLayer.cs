using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace DrakersChart;
internal class CrosshairLayer : Canvas
{
    private Point pos;
    private Boolean visible;
    private Boolean allowCrosshair = true;

    public Brush LineBrush { get; set; } = Brushes.Red;
    public Double LineThickness { get; set; } = 1;
    
    public Int32 BottomMargin { get; set; } = 0;

    public Boolean AllowCrosshair
    {
        get => this.allowCrosshair;
        set
        {
            this.allowCrosshair = value;
            InvalidateVisual();
        }
    }
     
    public CrosshairLayer()
    {
        this.Background = Brushes.Transparent;
        this.SnapsToDevicePixels = true;
        this.UseLayoutRounding = true;
        RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
    }
    
    /// <summary>
    /// 마우스 위치 업데이트
    /// </summary>
    /// <param name="position">마우스 위치</param>
    public void UpdatePosition(Point position)
    {
        this.pos = position;
        this.visible = this.allowCrosshair;
        InvalidateVisual();
    }
    
    /// <summary>
    /// 십자선 숨기기
    /// </summary>
    public void HideCrosshair()
    {
        this.visible = false;
        InvalidateVisual();
    }
    
    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        if (!this.visible)
        {
            return;
        }

        var pen = new Pen(this.LineBrush, this.LineThickness);
        pen.Freeze();
        
        dc.PushGuidelineSet(new GuidelineSet(
            [this.pos.X + 0.5],
            [this.pos.Y + 0.5]));

        dc.DrawLine(pen, new Point(this.pos.X, 0), new Point(this.pos.X, this.ActualHeight - this.BottomMargin + 2));
        dc.DrawLine(pen, new Point(0, this.pos.Y), new Point(this.ActualWidth, this.pos.Y));

        dc.Pop();
    }
}