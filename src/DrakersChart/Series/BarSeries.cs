using DrakersChart.Axis;
using DrakersChart.Legend;
using SkiaSharp;

namespace DrakersChart.Series;
public class BarSeries : IChartSeries<SeriesData>
{
    private readonly List<SeriesData> dataList = [];
    private readonly Dictionary<Int64, SeriesData> dataDic = new();
    
    public String SeriesName { get; set; } = String.Empty;
    public SKColor SeriesColor { get; set; } = SKColors.Black;
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
    public Single BottomMarginRatio => 0.01f;
    public AxisYGuideLocation AxisYGuideLocation { get; set; } = AxisYGuideLocation.Right;

    public ChartPane? Owner { get; set; }

    public IBarColorSelector ColorSelector { get; set; } = new BarColorSelector();

    public void Draw(SKCanvas canvas, AxisYScale yScale, AxisXDrawRegion[] drawRegions)
    {
        if (!this.isVisible)
        {
            return;
        }
        
        for (Int32 index = 0; index < drawRegions.Length - 1; index++)
        {
            var eachRegion = drawRegions[index];
            var data = this.dataDic[eachRegion.X];
            DrawBar(canvas, eachRegion, yScale, data);
        }
    }

    private void DrawBar(SKCanvas canvas, AxisXDrawRegion region, AxisYScale yScale, SeriesData data)
    {
        if (data.Value == null)
        {
            return;
        }

        Single bodyTop = (Single)data.Value;
        Double bodyBottom = 0;
        Single bodyTopY = (Single)yScale.ConvertToTarget(bodyTop);
        Double bodyBottomY = yScale.ConvertToTarget(bodyBottom);
        Single bodyWidth = (Int32)(region.Width * 0.8f);
        if ((Int32)bodyWidth % 2 == 1)
        {
            bodyWidth -= 1;
        }
        
        if (bodyWidth < 1)
        {
            bodyWidth = 1;
        }
        
        Single bodyHeight = (Single)(bodyBottomY - bodyTopY);
        if (bodyHeight < 1)
        {
            bodyHeight = 1;
        }
        
        Single bodyLeft = (Int32)(region.Center - bodyWidth / 2) + 0.5f;
        if (bodyLeft < region.Left)
        {
            bodyLeft += 1;
        }

        var paint = this.ColorSelector.GetBarColor(data); 
        if (Math.Abs(data.Value.Value - 0) > 0)
        {
            canvas.DrawRect(bodyLeft, bodyTopY, bodyWidth, bodyHeight, paint.line);
            canvas.DrawRect(bodyLeft, bodyTopY, bodyWidth, bodyHeight, paint.fill);
        }
        else
        {
            canvas.DrawLine(bodyLeft, bodyTopY, bodyLeft + bodyWidth, bodyTopY, paint.line);
        }
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

        Double min = 0;
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