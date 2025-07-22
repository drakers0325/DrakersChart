using SkiaSharp;

namespace DrakersChart.Series;
public interface IBarColorSelector
{
    (SKPaint fill, SKPaint line) GetBarColor(SeriesData data);
}