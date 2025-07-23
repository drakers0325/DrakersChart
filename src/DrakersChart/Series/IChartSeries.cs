using DrakersChart.Axis;
using DrakersChart.Legend;
using SkiaSharp;

namespace DrakersChart.Series;
public interface IChartSeries
{
    String SeriesName { get; }
    SKColor SeriesColor { get; }
    Boolean IsVisible { get; set; }
    Single TopMarginRatio { get; }
    Single BottomMarginRatio { get; }
    AxisYGuideLocation AxisYGuideLocation { get; set; }
    ChartPane? Owner { get; set; }

    void Draw(SKCanvas canvas, AxisYScale yScale, AxisXDrawRegion[] drawRegions);
    Int64[] GetAxisXValues();
    Range GetAxisYRange(Int64[] xAxisValues);
    SeriesLegendInfo[] GetSeriesLegendInfo();
}

public interface IChartSeries<in T> : IChartSeries
{
    void AddData(T[] data);
}