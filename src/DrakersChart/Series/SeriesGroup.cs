using DrakersChart.Axis;
using DrakersChart.Legend;
using SkiaSharp;

namespace DrakersChart.Series;
public class SeriesGroup : IChartSeries
{
    private readonly List<IChartSeries> seriesList = [];

    public String SeriesName { get; set; } = String.Empty;
    public SKColor SeriesColor { get; } = SKColors.Black;
    private Boolean isVisible = true;

    public Boolean IsVisible
    {
        get => this.isVisible;
        set
        {
            Boolean prev = this.isVisible;
            this.isVisible = value;

            if (prev != this.isVisible)
            {
                this.Owner?.RefreshChart();
            }
        }
    }
    public Single TopMarginRatio { get; private set; }
    public Single BottomMarginRatio { get; private set; }

    private AxisYGuideLocation axisYGuideLocation = AxisYGuideLocation.Right;

    public AxisYGuideLocation AxisYGuideLocation
    {
        get => this.axisYGuideLocation;
        set
        {
            this.axisYGuideLocation = value;
            foreach (var eachSeries in this.seriesList)
            {
                eachSeries.AxisYGuideLocation = this.axisYGuideLocation;
            }
        }
    }

    public ChartPane? Owner { get; set; }

    public void Draw(SKCanvas canvas, AxisYScale yScale, AxisXDrawRegion[] drawRegions)
    {
        if (!this.isVisible)
        {
            return;
        }
        foreach (var eachSeries in this.seriesList)
        {
            eachSeries.Draw(canvas, yScale, drawRegions);
        }
    }

    public Int64[] GetAxisXValues()
    {
        if (this.seriesList.Count == 0)
        {
            return [];
        }

        return this.seriesList
            .SelectMany(s => s.GetAxisXValues())
            .ToArray();
    }

    public Range GetAxisYRange(Int64[] xAxisValues)
    {
        if (this.seriesList.Count == 0)
        {
            return new Range(0, 0);
        }

        var ranges = this.seriesList.Select(s => s.GetAxisYRange(xAxisValues)).ToArray();
        return new Range(ranges.Min(r => r.Min), ranges.Max(r => r.Max));
    }

    public SeriesLegendInfo[] GetSeriesLegendInfo()
    {
        var legend1 = new SeriesLegendInfo[] { new(this.SeriesName, this.SeriesColor) };
        var legend2 = this.seriesList.Select(s => new SeriesLegendInfo(s.SeriesName, s.SeriesColor)).ToArray();
        return legend1.Concat(legend2).ToArray();
    }

    public void AddSeries(IChartSeries series)
    {
        this.seriesList.Add(series);
        SetTopBottomMarginRatio();
    }

    private void SetTopBottomMarginRatio()
    {
        this.TopMarginRatio = this.seriesList.Max(s => s.TopMarginRatio);
        this.BottomMarginRatio = this.seriesList.Min(s => s.BottomMarginRatio);
    }
}