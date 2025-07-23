using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using DrakersChart.Series;
using SkiaSharp;

namespace DrakersChart.Legend;
public class SeriesLegend : CheckBox
{
    private readonly TextBlock title = new();
    
    public IChartSeries Series { get; private set; }

    public SeriesLegend(IChartSeries series)
    {
        this.title.VerticalAlignment = VerticalAlignment.Center;
        this.title.HorizontalAlignment = HorizontalAlignment.Left;
        this.Content = this.title;
        this.Series = series;
        
        foreach (var eachInfo in series.GetSeriesLegendInfo())
        {
            AddTitle(eachInfo.Name, eachInfo.Color);
        }

        this.IsChecked = this.Series.IsVisible;

        this.Checked += (_, _) => { this.Series.IsVisible = true; };
        this.Unchecked += (_, _) => { this.Series.IsVisible = false; };
    }

    public void ClearTitle()
    {
        this.title.Inlines.Clear();
    }

    public void AddTitle(String title, SKColor color)
    {
        var run = new Run
        {
            Text = title + " ",
            Foreground = new SolidColorBrush(Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue)),
        };
        
        this.title.Inlines.Add(run);
    }
}