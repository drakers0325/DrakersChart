using DrakersChart.Axis;
using SkiaSharp;

namespace DrakersChart.Series;
public interface IChartSeries
{
    Single TopMarginRatio { get; }
    Single BottomMarginRatio { get; }
    AxisYGuideLocation AxisYGuideLocation { get; }
    ChartPane? Owner { get; set; }

    void Draw(SKCanvas canvas, AxisYScale yScale, AxisXDrawRegion[] drawRegions);
    Int64[] GetAxisXValues();
    Range GetAxisYRange(Int64[] xAxisValues);
}

public interface IChartSeries<in T> : IChartSeries
{
    void AddData(T[] data);
}