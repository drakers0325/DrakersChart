using System.Windows;
using SkiaSharp;

namespace DrakersChart.Axis;
public class AxisYGuideView
{
    private readonly SKPaint backgroundPaint = new()
    {
        Color = new SKColor(214, 226, 239),
        IsAntialias = false,
        Style = SKPaintStyle.Fill
    };

    private readonly SKPaint gridPaint = new()
    {
        Color = new SKColor(149, 149, 149),
        IsAntialias = false,
        Style = SKPaintStyle.Stroke,
        StrokeWidth = 1
    };

    private readonly SKPaint fontPaint = new()
    {
        Color = SKColors.Black,
        IsAntialias = false,
        Style = SKPaintStyle.Fill,
        StrokeWidth = 1
    };

    private readonly SKFont guideFont = new()
    {
        Size = 12,
        Typeface = SKTypeface.FromFamilyName("맑은 고딕"),
    };

    private readonly SKPaint guidePaint = new()
    {
        Color = new SKColor(255, 223, 144),
        IsAntialias = false,
        Style = SKPaintStyle.Fill,
        StrokeWidth = 1
    };

    private Rect region;

    public Boolean UseGuideView { get; set; } = true;
    public Boolean UseUnitString { get; set; }
    public AxisYGuideLocation Location { get; set; } = AxisYGuideLocation.Right;
    public Int32 Width { get; set; } = 80;

    public Boolean IsDrawGrid { get; set; } = true;

    public Boolean IsMouseHover(Point position)
    {
        return this.region.Contains(position);
    }

    public void DrawView(SKCanvas canvas, AxisYScale scale, AxisYGridManager gridManager, Double totalWidth, Double totalHeight, Double bottomMargin)
    {
        Single startX = (Single)(this.Location == AxisYGuideLocation.Left ? 0 : totalWidth - this.Width);
        Single drawHeight = (Single)(totalHeight - (bottomMargin - (bottomMargin == 0 ? 0 : +2)));
        DrawBackGround(canvas, startX, drawHeight);
        DrawGuideLines(canvas, scale, gridManager, startX, drawHeight);
    }

    private void DrawBackGround(SKCanvas canvas, Single x, Single height)
    {
        this.region = new Rect(x, 0, this.Width, height);
        canvas.DrawRect(x, 0, this.Width, height - 2, this.backgroundPaint);
        Single lineX = this.Location == AxisYGuideLocation.Left ? this.Width : x;
        canvas.DrawLine(lineX, 0, lineX, height - 2, Chart.BorderPaint);
    }

    private void DrawGuideLines(SKCanvas canvas, AxisYScale scale, AxisYGridManager gridManager, Single axisViewX, Single drawHeight)
    {
        var guideLines = gridManager.GuideLines;
        Double samplingValue = gridManager.SamplingValue;
        if (guideLines.Length > 0)
        {
            Int32 minorCount = samplingValue > 2 ? 5 : 2;
            Double minorInterval = samplingValue / minorCount;
            DrawMinorGuideLine(canvas, scale, axisViewX, guideLines[0].Value - samplingValue, guideLines[0].Value, minorInterval);
            for (Int32 index = 0; index < guideLines.Length; index++)
            {
                var currentLine = guideLines[index];
                Double nextLineValue = index < guideLines.Length - 1 ? guideLines[index + 1].Value : currentLine.Value + samplingValue;
                DrawMinorGuideLine(canvas, scale, axisViewX, currentLine.Value, nextLineValue, minorInterval);

                Single y = (Int32)currentLine.Location + 0.5f;
                Single startX = this.Location == AxisYGuideLocation.Left ? this.Width - 9 : axisViewX;
                Single endX = this.Location == AxisYGuideLocation.Left ? this.Width : axisViewX + 9;
                canvas.DrawLine(new SKPoint(startX, y), new SKPoint(endX, y), this.gridPaint);
                if ((y + 10 < drawHeight) && (y - 10 > 0))
                {
                    String guideString = CreateAxisYGuideValueString(currentLine.Value, this.UseUnitString);
                    Single textWidth = this.guideFont.MeasureText(guideString, out var _, this.fontPaint);
                    Single textX = this.Location == AxisYGuideLocation.Left ?
                        startX - 5 - textWidth :
                        startX + 13;
                    canvas.DrawText(guideString, textX, y + 4, this.guideFont, this.fontPaint);
                }
            }
        }
    }

    public static String CreateAxisYGuideValueString(Double value, Boolean useUnitString)
    {
        Double quotient = value / (useUnitString ? 1000000 : 1);
        Double remainder = Math.Abs(quotient % 1);
        String guideString;
        if (remainder == 0)
        {
            guideString = quotient.ToString("N0");
        }
        else
        {
            Int32 pointRemainder = (Int32)(remainder * 100 % 10);
            guideString = pointRemainder == 0 ? quotient.ToString("N1") : quotient.ToString("N2");
        }

        return useUnitString ? $"{guideString}M" : guideString;
    }

    private void DrawMinorGuideLine(SKCanvas canvas, AxisYScale scale, Single axisViewX, Double start, Double end, Double interval)
    {
        Double loc = start + interval;
        while (loc < end)
        {
            if (scale.SourceMin <= loc && loc <= scale.SourceMax)
            {
                Single y = (Int32)scale.ConvertToTarget(loc) + 0.5f;
                Single startX = this.Location == AxisYGuideLocation.Left ? this.Width - 4 : axisViewX;
                Single endX = this.Location == AxisYGuideLocation.Left ? this.Width : axisViewX + 4;
                canvas.DrawLine(new SKPoint(startX, y), new SKPoint(endX, y), this.gridPaint);
            }

            loc += interval;
        }
    }
    
    public void DrawCurrentGuideValue(SKCanvas canvas, AxisYScale scale, Double y, Single totalWidth)
    {
        if (scale.SourceMax - scale.SourceMin == 0)
        {
            return;
        }

        Double value = scale.ConvertToSource(y);
        String guideString = CreateAxisYGuideValueString(value, false);
        this.guideFont.MeasureText(guideString, out var rect, this.fontPaint);
        Single topY = (Single)y - 8;
        Single rectWidth = this.Width;
        Single rectHeight = rect.Height + 4;

        Single startX = this.Location == AxisYGuideLocation.Left ? 0 : totalWidth - this.Width + 2;
        canvas.DrawRect(startX, topY, rectWidth, rectHeight, this.guidePaint);
        canvas.DrawRect(startX, topY, rectWidth, rectHeight, this.gridPaint);
        canvas.DrawText(guideString, startX + 2, topY + 13f, this.guideFont, this.fontPaint);
    }
}