using DrakersChart.Axis;
using SkiaSharp;

namespace DrakersChart.Series;
public class CandleStickSeries : IChartSeries
{
    private readonly SKPaint bullPaint = new()
    {
        Color = new SKColor(231, 25, 9),
        IsAntialias = false,
        Style = SKPaintStyle.Fill
    };
    private readonly SKPaint bearPaint = new()
    {
        Color = new SKColor(17, 91, 203),
        IsAntialias = false,
        Style = SKPaintStyle.Fill
    };
    
    private readonly List<CandleData> dataList = [];
    private readonly Dictionary<Int64, CandleData> dataDic = new();

    public Double Min { get; private set; }
    public Double Max { get; private set; }

    public ChartPane? Owner { get; set; } = null;

    public void Draw(SKCanvas canvas, AxisYScale yScale, AxisXDrawRegion[] drawRegions)
    {
        foreach (var eachRegion in drawRegions)
        {
            var data = this.dataDic[eachRegion.X];
            DrawCandle(canvas, eachRegion, yScale, data);
        }
    }

    private void DrawCandle(SKCanvas canvas, AxisXDrawRegion xRegion, AxisYScale yScale, CandleData data)
    {
        Single bodyTop = (Single)Math.Max(data.OpenPrice, data.ClosePrice);
        Double bodyBottom = Math.Min(data.OpenPrice, data.ClosePrice);
        Single bodyTopY = (Single)yScale.ConvertToTarget(bodyTop);
        Double bodyBottomY = yScale.ConvertToTarget(bodyBottom);
        Single bodyWidth = (Int32)(xRegion.Width * 0.8f);
        if (bodyWidth < 1)
        {
            bodyWidth = 1;
        }
        Single bodyHeight = (Single)(bodyBottomY - bodyTopY);
        if (bodyHeight < 1)
        {
            bodyHeight = 1;
        }

        Single center = xRegion.Center;
        Single bodyLeft = center - (bodyWidth / 2);

        Single lineTopY = (Single)yScale.ConvertToTarget(data.HighPrice);
        Single lineBottomY = (Single)yScale.ConvertToTarget(data.LowPrice);
        Single lineLeft = center - 0.5f;
        
        var paint = data.ClosePrice > data.OpenPrice ? this.bullPaint : this.bearPaint;
        
        canvas.DrawRect(bodyLeft, bodyTopY, bodyWidth, bodyHeight, paint);
        canvas.DrawLine(lineLeft, lineTopY, lineLeft, lineBottomY, paint);
    }

    public Int64[] GetAxisXValues()
    {
        return this.dataList.Select(v => v.DateTime.ToBinary()).ToArray();
    }

    public void AddData(CandleData[] data)
    {
        foreach (var eachData in data)
        {
            this.dataDic.TryAdd(eachData.DateTime.ToBinary(), eachData);
        }
        this.dataList.Clear();
        this.dataList.AddRange(this.dataDic.Values.OrderBy(v => v.DateTime));
        this.dataList.Sort((a, b) => a.DateTime.CompareTo(b.DateTime));
        SetDataLink();
        this.Min = this.dataList.Min(v => v.LowPrice);
        this.Max = this.dataList.Max(v => v.HighPrice);
        
        this.Owner?.OwnerChart.AxisXDataManager.AddData(data.Select(v => v.DateTime.ToBinary()).ToArray());
        this.Owner?.RefreshChart();
    }

    private void SetDataLink()
    {
        var prev = this.dataList[0];
        for (Int32 index = 1; index < this.dataList.Count; index++)
        {
            this.dataList[index].PreviousData = prev;
            prev.NextData = this.dataList[index];
        }
    }
}