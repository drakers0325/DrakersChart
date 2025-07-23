namespace DrakersChart.Axis;
public struct AxisYGuideLine(Single location, Single length, Double value)
{
    public Single Location { get; private set; } = location;
    public Single Length { get; private set; } = length;
    public Double Value { get; private set; } = value;
}