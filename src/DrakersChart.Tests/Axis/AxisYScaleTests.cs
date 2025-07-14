using DrakersChart.Axis;
using FluentAssertions;

namespace DrakersChart.Tests.Axis;
public class AxisYScaleTests
{
    [Theory]
    [InlineData(-10, 100, 100, 1000, -10, 1000)]
    [InlineData(-10, 100, 100, 1000, 100, 100)]
    [InlineData(-10, 100, 100, 1000, 0, 918.1818181818181)]
    public void Convert_SourceValue_To_YCoordinate(Double sourceMin, Double sourceMax, Double yTop, Double yBottom, Double sourceValue, Double expectedValue)
    {
        var scale = new AxisYScale()
        {
            SourceMin = sourceMin,
            SourceMax = sourceMax,
            TargetMin = yBottom,
            TargetMax = yTop,
        };

        scale.ConvertToTarget(sourceValue).Should().Be(expectedValue);
    }
}