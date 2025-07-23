using SkiaSharp;

namespace DrakersChart.Legend;
public struct SeriesLegendInfo(String name, SKColor color)
{
    public String Name { get; private set; } = name;
    public SKColor Color { get; private set; } = color;
}