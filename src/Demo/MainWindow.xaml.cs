using System.IO;
using System.Reflection;
using System.Windows;
using DrakersChart.Axis;
using DrakersChart.Series;
using SkiaSharp;

namespace Demo;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public class DayStockTradeData
    {
        public DateOnly Index { get; set; }

        public Double OpenPrice { get; set; }
        public Double HighPrice { get; set; }
        public Double LowPrice { get; set; }
        public Double ClosePrice { get; set; }
        public Double TradingVolume { get; set; }
        public Double TradingValue { get; set; }
    }

    public MainWindow()
    {
        InitializeComponent();
        InitializeChart();
    }

    private void InitializeChart()
    {
        this.demoChart.AddChart();
        this.demoChart.AddChart();
        this.demoChart.AddChart();

        String data = ReadDataTextFile();
        var candleData = JsonHelper.Deserialize<List<DayStockTradeData>>(data).Select(v =>
            new CandleData(v.Index.ToDateTime(TimeOnly.MinValue))
            {
                OpenPrice = v.OpenPrice,
                HighPrice = v.HighPrice,
                LowPrice = v.LowPrice,
                ClosePrice = v.ClosePrice,
                Volume = v.TradingVolume
            }).ToArray();

        AddCandleSeries(candleData);
        AddSMA(candleData);
        AddBarChart(candleData);
        AddOBV(candleData);

        this.demoChart.SetChartPaneHeightRatio([0.7, 0.15, 0.15]);
        this.demoChart.SetDisplayRange(candleData.Length - 240, 240);
    }

    private void AddCandleSeries(CandleData[] candleData)
    {
        var series = new CandleStickSeries()
        {
            SeriesName = "캔들",
            AxisYGuideLocation = AxisYGuideLocation.Right,
        };
        series.AddData(candleData);

        this.demoChart.ChartPanes[0].AddSeries(series);
    }

    private void AddBarChart(CandleData[] candleData)
    {
        var series = new BarSeries()
        {
            SeriesName = "거래량",
            AxisYGuideLocation = AxisYGuideLocation.Right,
        };
        var data = candleData.Select(v => new SeriesData(v.DateTime.ToBinary()) { Value = v.Volume }).ToArray();
        series.AddData(data);

        this.demoChart.ChartPanes[1].AddSeries(series);
    }

    private void AddSMA(CandleData[] candleData)
    {
        var sma20 = CreateSMA(candleData, 20);
        var sma60 = CreateSMA(candleData, 60);
        var sma120 = CreateSMA(candleData, 120);
        var sma240 = CreateSMA(candleData, 240);

        var group = new SeriesGroup
        {
            SeriesName = "SMA",
        };
        group.AddSeries(CreateLineSeries(sma20, SKColors.Aqua, "20"));
        group.AddSeries(CreateLineSeries(sma60, SKColors.Orange, "60"));
        group.AddSeries(CreateLineSeries(sma120, SKColors.Green, "120"));
        group.AddSeries(CreateLineSeries(sma240, SKColors.Black, "240"));
        
        this.demoChart.ChartPanes[0].AddSeries(group);
    }

    private void AddOBV(CandleData[] candleData)
    {
        var obv = CreateOBV(candleData);
        AddLineSeries(2, obv, "OBV", SKColors.DarkSlateBlue);
    }

    private static LineSeries CreateLineSeries(SeriesData[] data, SKColor color, String name)
    {
        var series = new LineSeries
        {
            SeriesName = name,
            SeriesColor = color
        };
        series.AddData(data);
        return series;
    }

    private void AddLineSeries(Int32 chartPaneIndex, SeriesData[] data, String name, SKColor color)
    {
        var series = new LineSeries
        {
            SeriesName = name,
            SeriesColor = color
        };
        series.AddData(data);

        this.demoChart.ChartPanes[chartPaneIndex].AddSeries(series);
    }

    private static String ReadDataTextFile()
    {
        var asm = Assembly.GetExecutingAssembly();
        using var stream = asm.GetManifestResourceStream("Demo.Resources.data.txt");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private SeriesData[] CreateSMA(CandleData[] data, Int32 period)
    {
        Double[] closes = data.Select(d => d.ClosePrice).ToArray();

        var smaList = data.Select(d => new SeriesData(d.DateTime.ToBinary())).ToArray();

        Double sum = 0;
        for (Int32 i = 0; i < closes.Length; i++)
        {
            sum += closes[i];
            if (i >= period)
            {
                sum -= closes[i - period];
            }

            if (i + 1 >= period)
            {
                smaList[i].Value = sum / period;
            }
            else
            {
                smaList[i].Value = null;
            }
        }

        return smaList;
    }

    private SeriesData[] CreateOBV(CandleData[] data)
    {
        var obvList = data.Select(d => new SeriesData(d.DateTime.ToBinary())).ToArray();

        Double sum = data[0].Volume;
        obvList[0].Value = sum;
        var prev = data[0];
        for (Int32 index = 1; index < data.Length; index++)
        {
            var eachData = data[index];
            Double addVolume = eachData.ClosePrice > prev.ClosePrice ? eachData.Volume :
                Math.Abs(eachData.ClosePrice - prev.OpenPrice) < 0 ? 0 : -eachData.Volume;
            sum += addVolume;
            obvList[index].Value = sum;

            prev = eachData;
        }

        return obvList;
    }
}