using SkiaSharp;

namespace DrakersChart.Series;
public readonly struct HintInfo(String name, SKColor color, params HintValue[] values)
{
    public String SeriesName { get; } = name;
    public SKColor Color { get; } = color;
    public HintValue[] Values { get; } = values;
}