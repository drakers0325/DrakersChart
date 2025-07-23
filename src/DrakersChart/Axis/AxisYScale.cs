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

    public Double ConvertToSource(Double value)
    {
        if (Math.Abs(this.TargetMax - this.TargetMin) < 0)
        {
            throw new ApplicationException($"TargetMax와 TargetMin이 같으면 변환할 수 없습니다. (min:{this.TargetMin}, max:{this.TargetMax}), value:{value}");
        }
        
        Double t = (value - this.TargetMin) / (this.TargetMax - this.TargetMin); 
        return this.SourceMin + t * (this.SourceMax - this.SourceMin);
    }
}