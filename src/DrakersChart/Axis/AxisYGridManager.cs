namespace DrakersChart.Axis;
public class AxisYGridManager(AxisYScale scale)
{
    public AxisYGuideLine[] GuideLines { get; private set; } = [];
    public Double SamplingValue { get; private set; }

    public void Update()
    {
        this.GuideLines = CreateAxisGuideLine(scale, scale.TargetMin, out Double samplingValue);
        this.SamplingValue =  samplingValue;
    }
    
    private static AxisYGuideLine[] CreateAxisGuideLine(AxisYScale scale, Double drawHeight, out Double samplingValue)
    {
        if (scale.SourceMax - scale.SourceMin != 0 && drawHeight > 0)
        {
            var lineList = new List<AxisYGuideLine>();
            samplingValue = CreateSamplingValue(scale, drawHeight);
            Double standardStartValue = scale.SourceMin / samplingValue;
            Double guideValue = scale.SourceMin < 0 ?
                (Int64)standardStartValue * samplingValue :
                (Int64)standardStartValue * samplingValue + samplingValue;
            Double prevLoc = drawHeight;
            while (guideValue < scale.SourceMax)
            {
                Double loc = scale.ConvertToTarget(guideValue);
                lineList.Add(new AxisYGuideLine((Single)loc, (Single)(prevLoc - loc), guideValue));
                guideValue += samplingValue;
            }

            return lineList.ToArray();
        }
        else
        {
            samplingValue = 0;
            return [];
        }
    }

    private static Double CreateSamplingValue(AxisYScale scale, Double drawHeight)
    {
        Double axisDiff = scale.SourceMax - scale.SourceMin;
        Double roughValue = (Int64)(axisDiff / (drawHeight / 35));
        Double samplingValue = ReviseSamplingValue(roughValue);
        if (samplingValue * 1.2 >= scale.SourceMax)
        {
            Int64 quotient = (Int64)(samplingValue / axisDiff);
            quotient = quotient % 2 == 1 ? quotient : quotient + 1;
            samplingValue /= quotient + 1;
        }

        Int64 interval = (Int64)(drawHeight * (samplingValue / axisDiff));

        if (interval <= 25 && drawHeight >= 70)
        {
            samplingValue *= 2;
        }

        if (interval >= 60 && drawHeight >= 70)
        {
            samplingValue /= 2;
        }

        return Math.Abs(samplingValue);
    }

    private static Double ReviseSamplingValue(Double samplingValue)
    {
        return samplingValue < 10 ?
            (samplingValue > 5 ? 10 : samplingValue is > 3 and <= 5 ? 5 : samplingValue > 1 ? 2.5 : 1) :
            ReviseSamplingValue((Int64)samplingValue / 10.0) * 10;
    }
}