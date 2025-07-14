namespace DrakersChart.Axis;
public class AxisXDrawRegionManager
{
    private readonly Chart chart;
    private readonly AxisXDataManger dataManager;
    private readonly List<AxisXDrawRegion> drawRegionList = [];
    
    public Int32 StartIndex { get; private set; }
    public Int32 Count { get; private set; }

    private Double height;

    public Double Height
    {
        get => this.height;
        set
        {
            this.height = value;
            SetDrawRegion();
        }
    }
    
    private Double width;

    public Double Width
    {
        get => this.width;
        set
        {
            this.width = value;
            SetDrawRegion();
        }
    }

    public AxisXDrawRegion[] DrawRegions => this.drawRegionList.ToArray();

    public AxisXDrawRegionManager(Chart chart, AxisXDataManger dataManager)
    {
        this.chart = chart;
        this.dataManager = dataManager;
        this.dataManager.DataUpdated += (_, _) =>
        {
            SetDrawRegion();
        };
    }

    public void SetDisplayRange(Int32 startIndex, Int32 count)
    {
        this.StartIndex = startIndex;
        Int32 diff = this.dataManager.DataCount - this.StartIndex;
        this.Count = count > diff ? diff : count;
        SetDrawRegion();
    }

    private void SetDrawRegion()
    {
        if (this.width == 0 || this.height == 0 || this.Count == 0)
        {
            return;
        }
        this.drawRegionList.Clear();

        Single eachWidth = (Single)(this.width / this.Count);
        Single left = 0;
        Int32 end = this.StartIndex + this.Count;
        var axisXData = this.dataManager.Data;
        for (Int32 index = this.StartIndex; index < end; index++)
        {
            this.drawRegionList.Add(new AxisXDrawRegion(axisXData[index].Value, left, left + eachWidth));
            left += eachWidth;
        }
    }
}