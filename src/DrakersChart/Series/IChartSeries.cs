using DrakersChart.Axis;
using SkiaSharp;

namespace DrakersChart.Series;
public interface IChartSeries
{
    Double Min { get; }
    Double Max { get; }
    
    void Draw(SKCanvas canvas, AxisYScale yScale, AxisXDrawRegion[] drawRegions);
    Int64[] GetAxisXValues();
}