﻿using DrakersChart.Axis;
using DrakersChart.Legend;
using SkiaSharp;

namespace DrakersChart.Series;
public class CandleStickSeries : IChartSeries<CandleData>
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

    private readonly SKPaint bullLinePaint = new()
    {
        Color = new SKColor(231, 25, 9),
        IsAntialias = false,
        Style = SKPaintStyle.Stroke
    };

    private readonly SKPaint bearLinePaint = new()
    {
        Color = new SKColor(17, 91, 203),
        IsAntialias = false,
        Style = SKPaintStyle.Stroke
    };

    private readonly List<CandleData> dataList = [];
    private readonly Dictionary<Int64, CandleData> dataDic = new();
    private readonly Dictionary<SKPath, Int64> pathDic = new();

    public String SeriesName { get; set; } = String.Empty;
    public SKColor SeriesColor { get; set; } = SKColors.Black;
    private Boolean isVisible = true;

    public SKColor BullColor
    {
        get => this.bullPaint.Color;
        set
        {
            this.bullPaint.Color = value;
            this.bullLinePaint.Color = value;
        }
    }

    public SKColor BearColor
    {
        get => this.bearPaint.Color;
        set
        {
            this.bearPaint.Color = value;
            this.bearLinePaint.Color = value;
        }
    }

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

    public Single TopMarginRatio => 0.1f;
    public Single BottomMarginRatio => 0.05f;

    public AxisYGuideLocation AxisYGuideLocation { get; set; } = AxisYGuideLocation.Right;

    public ChartPane? Owner { get; set; } = null;

    public Range GetAxisYRange(Int64[] xAxisValues)
    {
        if (this.dataDic.Values.Count == 0)
        {
            return new Range(0, 0);
        }

        var first = this.dataDic[xAxisValues[0]];
        Double min = first.LowPrice;
        Double max = first.HighPrice;

        for (Int32 index = 1; index < xAxisValues.Length; index++)
        {
            var eachData = this.dataDic[xAxisValues[index]];
            if (min > eachData.LowPrice)
            {
                min = eachData.LowPrice;
            }

            if (max < eachData.HighPrice)
            {
                max = eachData.HighPrice;
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

        var values = new[]
        {
            new HintValue("시가", data.OpenPrice, SKColors.Black), new HintValue("고가", data.HighPrice, this.BullColor),
            new HintValue("저가", data.LowPrice, this.BearColor), new HintValue("종가", data.ClosePrice, SKColors.Black)
        };
        return new HintInfo(this.SeriesName, this.SeriesColor, values);
    }

    public void Draw(SKCanvas canvas, AxisYScale yScale, AxisXDrawRegion[] drawRegions)
    {
        this.pathDic.Clear();
        if (!this.isVisible)
        {
            return;
        }

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

        Single bodyLeft = (Int32)(xRegion.Center - bodyWidth / 2) + 0.5f;
        if (bodyLeft < xRegion.Left)
        {
            bodyLeft += 1;
        }

        Single center = (bodyLeft + bodyLeft + bodyWidth) / 2;

        Single lineTopY = (Single)yScale.ConvertToTarget(data.HighPrice);
        Single lineBottomY = (Single)yScale.ConvertToTarget(data.LowPrice);

        var paint = data.ClosePrice > data.OpenPrice ? this.bullPaint : this.bearPaint;
        var linePaint = data.ClosePrice > data.OpenPrice ? this.bullLinePaint : this.bearLinePaint;

        if (Math.Abs(data.OpenPrice - data.ClosePrice) > 0)
        {
            canvas.DrawRect(bodyLeft, bodyTopY, bodyWidth, bodyHeight, linePaint);
            canvas.DrawRect(bodyLeft, bodyTopY, bodyWidth, bodyHeight, paint);
        }
        else
        {
            canvas.DrawLine(bodyLeft, bodyTopY, bodyLeft + bodyWidth, bodyTopY, linePaint);
        }

        var path = new SKPath();
        path.AddRect(new SKRect(bodyLeft - 0.5f, bodyTopY - 0.5f, bodyLeft + bodyWidth + 0.5f, bodyTopY + bodyHeight + 0.5f));

        if (Math.Abs(data.HighPrice - data.LowPrice) > 0)
        {
            canvas.DrawLine(center, lineTopY, center, lineBottomY, linePaint);
            path.AddRect(new SKRect(center - 0.5f, lineTopY - 0.5f, center + 0.5f, lineBottomY + 0.5f));
        }

        this.pathDic.Add(path, data.DateTime.ToBinary());
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
            prev = this.dataList[index];
        }
    }
}