namespace DrakersChart.Series;
public class SeriesData(Int64 index)
{
    public Int64 Index { get; } = index;
    public Double? Value { get; set; }
    
    internal SeriesData? PreviousData { get; set; }
    internal SeriesData? NextData { get; set; }
}