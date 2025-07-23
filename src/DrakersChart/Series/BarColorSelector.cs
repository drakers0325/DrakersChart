using SkiaSharp;

namespace DrakersChart.Series;
public class BarColorSelector : IBarColorSelector
{
    private readonly SKPaint bullPaint = new()
    {
        Color = new SKColor(231, 25, 9),
        IsAntialias = false,
        Style = SKPaintStyle.Fill
    };
    private readonly SKPaint bearPaint = new()
    {
        Color = new SKColor(17, 91, 203),
        IsAntialias = false,
        Style = SKPaintStyle.Fill
    };
    
    private readonly SKPaint bullLinePaint = new()
    {
        Color = new SKColor(231, 25, 9),
        IsAntialias = false,
        Style = SKPaintStyle.Stroke
    };
    private readonly SKPaint bearLinePaint = new()
    {
        Color = new SKColor(17, 91, 203),
        IsAntialias = false,
        Style = SKPaintStyle.Stroke
    };

    public (SKPaint fill, SKPaint line) GetBarColor(SeriesData data)
    {
        if (data.PreviousData == null || data.Value == null || data.PreviousData.Value == null || data.Value > data.PreviousData.Value)
        {
            return (this.bullPaint, this.bullLinePaint);
        }
        
        return (this.bearPaint, this.bearLinePaint);
    }
}