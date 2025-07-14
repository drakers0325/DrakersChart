using System.Globalization;

namespace DrakersChart.Axis;
public class AxisXData
{
    private String labelFormat = String.Empty;

    public String LabelFormat
    {
        get => this.labelFormat;
        set
        {
            this.labelFormat = value;
            SetLabelText();
        }
    }

    public Int64 Value { get; }
    public String Label { get; private set; } = String.Empty;

    private AxisXDataType dataType;

    public AxisXDataType DataType
    {
        get => this.dataType;
        set
        {
            this.dataType = value;
            SetLabelText();
        }
    }

    public AxisXData(Int64 value, AxisXDataType dataType = AxisXDataType.Integer)
    {
        this.Value = value;
        this.dataType = dataType;
        SetLabelText();
    }

    private void SetLabelText()
    {
        switch (this.dataType)
        {
            case AxisXDataType.Integer:
                this.Label = this.Value.ToString(this.labelFormat);
                break;
            case AxisXDataType.DateTime:
                var dt = DateTime.FromBinary(this.Value);
                this.Label = dt.ToString(this.labelFormat);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}