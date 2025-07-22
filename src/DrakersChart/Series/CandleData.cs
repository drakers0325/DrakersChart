namespace DrakersChart.Series;
public class CandleData(DateTime dateTime)
{
    public DateTime DateTime { get; } = dateTime;
    public Double OpenPrice { get; set; }
    public Double HighPrice { get; set; }
    public Double LowPrice { get; set; }
    public Double ClosePrice { get; set; }
    public Double Volume { get; set; }
    
    internal CandleData? PreviousData { get; set; }
    internal CandleData? NextData { get; set; }
}