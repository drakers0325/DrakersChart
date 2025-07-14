namespace DrakersChart.Axis;
public class AxisYScale
{
    public Double SourceMin { get; set; }
    public Double SourceMax { get; set; }
    public Double TargetMin { get; set; }
    public Double TargetMax { get; set; }

    public Double ConvertToTarget(Double value)
    {
        if (this.SourceMin > value || this.SourceMax < value)
        {
            throw new ApplicationException($"Source의 범위를 벗어났습니다(min:{this.SourceMin}, max:{this.SourceMax}), value:{value}");
        }

        Double t = (value - this.SourceMin) / (this.SourceMax - this.SourceMin); 
        return this.TargetMin + t * (this.TargetMax - this.TargetMin);
    }
}