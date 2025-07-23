namespace DrakersChart.Axis;
public class AxisXDataManger
{
    private readonly EventSet eventSet = new();
    private readonly List<AxisXData> dataList = [];
    private readonly Dictionary<Int64, AxisXData> dataDic = new();

    private String labelFormat = String.Empty;

    public AxisXData[] Data => this.dataList.ToArray();
    public Int32 DataCount => this.dataList.Count;

    public String LabelFormat
    {
        get => this.labelFormat;
        set
        {
            String prevFormat = this.labelFormat;
            this.labelFormat = value;
            foreach (var eachData in this.dataList)
            {
                eachData.LabelFormat = this.labelFormat;
            }

            if (!prevFormat.Equals(this.labelFormat) && this.dataList.Count > 0)
            {
                RaiseDataUpdatedEvent();
            }
        }
    }

    private AxisXDataType dataType;

    public AxisXDataType DataType
    {
        get => this.dataType;
        set
        {
            var prevType = this.dataType;
            this.dataType = value;
            foreach (var eachData in this.dataList)
            {
                eachData.DataType = this.dataType;
            }

            if (prevType != this.dataType && this.dataList.Count > 0)
            {
                RaiseDataUpdatedEvent();
            }
        }
    }

    private static readonly EventKey updatedKey = new(nameof(DataUpdated));

    public event EventHandler<EventArgs> DataUpdated
    {
        add => this.eventSet.Add(updatedKey, value);
        remove => this.eventSet.Remove(updatedKey, value);
    }

    private void RaiseDataUpdatedEvent()
    {
        this.eventSet.Raise(updatedKey, this, EventArgs.Empty);
    }

    public void AddData(Int64[] data)
    {
        Int32 addCount = data.Select(val => new AxisXData(val, this.dataType)).Count(eachData => this.dataDic.TryAdd(eachData.Value, eachData));

        if (addCount <= 0)
        {
            return;
        }

        this.dataList.Clear();
        this.dataList.AddRange(this.dataDic.Values.OrderBy(val => val.Value));
        RaiseDataUpdatedEvent();
    }
}