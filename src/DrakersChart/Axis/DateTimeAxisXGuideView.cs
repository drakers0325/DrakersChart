using System.Windows;
using SkiaSharp;

namespace DrakersChart.Axis;
public class DateTimeAxisXGuideView(AxisXGridManager gridManager)
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
        Color = new SKColor(77, 77, 77),
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
        Color = SKColors.Orange,
        IsAntialias = false,
        Style = SKPaintStyle.Fill,
        StrokeWidth = 1
    };

    private Rect region;

    public Int32 Height { get; } = 20;
    public Boolean IsDrawGrid { get; set; } = true;

    public Boolean IsMouseHover(Point position)
    {
        return this.region.Contains(position);
    }

    public void DrawView(SKCanvas canvas, Single topY, Double totalWidth, Double leftGuideWidth, Double rightGuideWidth)
    {
        DrawBackGround(canvas, topY, totalWidth);
        DrawGrid(canvas, topY);
    }

    private void DrawBackGround(SKCanvas canvas, Single topY, Double totalWidth)
    {
        this.region = new Rect(0, topY, totalWidth, this.Height);
        canvas.DrawRect(0, topY, (Single)totalWidth, this.Height, this.backgroundPaint);
        canvas.DrawLine(0, topY, (Single)totalWidth, topY, Chart.BorderPaint);
    }

    private void DrawGrid(SKCanvas canvas, Single topY)
    {
        for (Int32 index = 0; index < gridManager.Infos.Length; index++)
        {
            var eachInfo = gridManager.Infos[index];

            var x = DateTime.FromBinary(eachInfo.X);
            Single textWidth = this.guideFont.MeasureText(eachInfo.Label, out var bounds, this.fontPaint);
            
            if (textWidth < eachInfo.Width)
            {
                canvas.DrawText(eachInfo.Label, eachInfo.Coordinate + 2, topY + 13f, this.guideFont, this.fontPaint);
            }
            
            if (index > 0 && this.IsDrawGrid)
            {
                canvas.DrawLine(eachInfo.Coordinate, topY, eachInfo.Coordinate, topY + this.Height, this.gridPaint);
            }
        }
    }

    public void DrawCurrentGuideValue(SKCanvas canvas, AxisXDrawRegion region, Single x, Single topY, Single totalWidth)
    {
        var dt = DateTime.FromBinary(region.X);
        String text = dt.ToString("yyyy/MM/dd");
        Single textWidth = this.guideFont.MeasureText(text, out var bounds, this.fontPaint);
        Single drawX = x - (textWidth / 2) - 2;
        if (drawX < 0)
        {
            drawX = 0;
        }

        Single rectWidth = textWidth + 4;
        if (Math.Round(drawX + rectWidth) >= totalWidth)
        {
            drawX = totalWidth - rectWidth - 2;
        }
        
        
        canvas.DrawRect(drawX, topY, rectWidth, this.Height, this.guidePaint);
        canvas.DrawRect(drawX, topY, rectWidth, this.Height, this.gridPaint);
        canvas.DrawText(text, drawX + 2, topY + 13f, this.guideFont, this.fontPaint);
    }
}