using System.Windows.Media;
using SkiaSharp;

namespace DrakersChart;
public static class ExtensionMethods
{
    public static Color ToColor(this SKColor color)
    {
        return Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);
    }
}