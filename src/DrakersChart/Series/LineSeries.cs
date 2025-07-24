using DrakersChart.Axis;
using DrakersChart.Legend;
using SkiaSharp;

namespace DrakersChart.Series;
public class LineSeries : IChartSeries<SeriesData>
{
    private readonly List<SeriesData> dataList = [];
    private readonly Dictionary<Int64, SeriesData> dataDic = new();
    private readonly Dictionary<SKPath, Int64> pathDic = new();

    public String SeriesName { get; set; } = String.Empty;

    public SKColor SeriesColor
    {
        get => this.linePaint.Color;
        set => this.linePaint.Color = value;
    }

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

    public Single TopMarginRatio => 0.05f;
    public Single BottomMarginRatio => 0.05f;
    public AxisYGuideLocation AxisYGuideLocation { get; set; } = AxisYGuideLocation.Right;

    public ChartPane? Owner { get; set; }

    private readonly SKPaint linePaint = new()
    {
        Color = SKColors.Black,
        IsAntialias = true,
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 1,
    };

    public Single LineThickness
    {
        get => this.linePaint.StrokeWidth;
        set => this.linePaint.StrokeWidth = value;
    }

    public void Draw(SKCanvas canvas, AxisYScale yScale, AxisXDrawRegion[] drawRegions)
    {
        this.pathDic.Clear();
        if (!this.isVisible)
        {
            return;
        }

        for (Int32 index = 0; index < drawRegions.Length - 1; index++)
        {
            var eachRegion = drawRegions[index];
            var nextRegion = drawRegions[index + 1];
            var data = this.dataDic[eachRegion.X];
            DrawLine(canvas, eachRegion, nextRegion, yScale, data);
        }
    }

    private void DrawLine(SKCanvas canvas, AxisXDrawRegion region, AxisXDrawRegion nextRegion, AxisYScale yScale, SeriesData data)
    {
        if (data.Value == null)
        {
            return;
        }

        var path = new SKPath();

        if (data.PreviousData == null && data.NextData == null)
        {
            Single y = (Int32)yScale.ConvertToTarget(data.Value.Value) + 0.5f;
            Single x = (Int32)region.Center + 0.5f;
            canvas.DrawCircle(new SKPoint(x, y), 2, this.linePaint);
            path.AddCircle(x, y, 2);
        }
        else if (data.NextData is { Value: not null })
        {
            if (data.NextData.Index != nextRegion.X)
            {
                throw new ApplicationException($"다음 데이터의 X'{data.NextData.Index}'와 다음 구역의 X'{nextRegion.X}'가 다릅니다");
            }

            Single x1 = (Int32)region.Center + 0.5f;
            Single y1 = (Int32)yScale.ConvertToTarget(data.Value.Value) + 0.5f;
            Single x2 = (Int32)nextRegion.Center + 0.5f;
            Single y2 = (Int32)yScale.ConvertToTarget(data.NextData.Value.Value) + 0.5f;

            canvas.DrawLine(x1, y1, x2, y2, this.linePaint);
            path.AddRect(new SKRect(x1 - 0.5f, y1 - 0.5f, x2 + 0.5f, y2 + 0.5f));
        }

        this.pathDic.Add(path, data.Index);
    }

    public Int64[] GetAxisXValues()
    {
        return this.dataList.Select(x => x.Index).ToArray();
    }

    public Range GetAxisYRange(Int64[] xAxisValues)
    {
        if (this.dataDic.Values.Count == 0)
        {
            return new Range(0, 0);
        }

        Double[] values = xAxisValues.Select(x => this.dataDic[x].Value)
            .Where(x => x != null)
            .Select(x => x.Value)
            .ToArray();

        if (values.Length == 0)
        {
            return new Range(0, 0);
        }

        Double min = values[0];
        Double max = values[0];

        for (Int32 index = 1; index < values.Length; index++)
        {
            Double eachValue = values[index];

            if (min > eachValue)
            {
                min = eachValue;
            }

            if (max < eachValue)
            {
                max = eachValue;
            }
        }

        return new Range(min, max);
    }

    public SeriesLegendInfo[] GetSeriesLegendInfo()
    {
        return [new SeriesLegendInfo(this.SeriesName, this.SeriesColor)];
    }

    public Boolean IsMouseHover(Single x, Single y, out Int64 xValue)
    {
        foreach (var eachPair in this.pathDic.Where(eachPair => eachPair.Key.Contains(x, y)))
        {
            xValue = eachPair.Value;
            return true;
        }

        xValue = -1;
        return false;
    }

    public HintInfo GetHintInfo(Int64 xValue)
    {
        if (!this.dataDic.TryGetValue(xValue, out var data))
        {
            return new HintInfo(this.SeriesName, this.SeriesColor);
        }

        var values = new[] { new HintValue(this.SeriesName, data.Value, this.SeriesColor) };
        return new HintInfo(this.SeriesName, this.SeriesColor, values);
    }

    public void AddData(SeriesData[] data)
    {
        foreach (var eachData in data)
        {
            this.dataDic.TryAdd(eachData.Index, eachData);
        }

        this.dataList.Clear();
        this.dataList.AddRange(this.dataDic.Values.OrderBy(v => v.Index));
        this.dataList.Sort((a, b) => a.Index.CompareTo(b.Index));
        SetDataLink();

        this.Owner?.OwnerChart.AxisXDataManager.AddData(data.Select(v => v.Index).ToArray());
        this.Owner?.RefreshChart();
    }

    private void SetDataLink()
    {
        var prev = this.dataList[0];
        for (Int32 index = 1; index < this.dataList.Count; index++)
        {
            this.dataList[index].PreviousData = prev;
            prev.NextData = this.dataList[index];
            prev = this.dataList[index];
        }
    }
}