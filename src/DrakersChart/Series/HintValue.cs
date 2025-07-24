using SkiaSharp;

namespace DrakersChart.Series;
public readonly struct HintValue(String name, Double? value, SKColor color)
{
    public String Name { get; } = name;
    public Double? Value { get; } = value;
    public SKColor Color { get; } = color;
}