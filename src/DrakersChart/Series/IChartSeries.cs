using DrakersChart.Axis;
using SkiaSharp;

namespace DrakersChart.Series;
public interface IChartSeries
{
    Single TopMarginRatio { get; }
    Single BottomMarginRatio { get; }
    
    void Draw(SKCanvas canvas, AxisYScale yScale, AxisXDrawRegion[] drawRegions);
    Int64[] GetAxisXValues();
    Range GetAxisYRange(Int64[] xAxisValues);
}