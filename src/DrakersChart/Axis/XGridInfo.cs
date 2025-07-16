namespace DrakersChart.Axis;
public readonly struct XGridInfo(Int64 x, Single coordinate, Single width, String label)
{
    public Int64 X { get; } = x;
    public Single Coordinate  { get; } = coordinate;
    public Single Width { get; } = width;
    
    public String Label { get; } = label;
}