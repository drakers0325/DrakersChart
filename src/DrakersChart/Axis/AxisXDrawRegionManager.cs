namespace DrakersChart.Axis;
public class AxisXDrawRegionManager
{
    private readonly EventSet eventSet = new();
    private readonly AxisXDataManger dataManager;
    private readonly List<AxisXDrawRegion> drawRegionList = [];

    private static readonly EventKey regionUpdatedKey = new();

    public event EventHandler<EventArgs> DrawRegionsUpdated
    {
        add => this.eventSet.Add(regionUpdatedKey, value);
        remove => this.eventSet.Remove(regionUpdatedKey, value);
    }

    public Int32 StartIndex { get; private set; }
    public Int32 DisplayCount { get; private set; }
    public Int32 DrawRegionCount => this.drawRegionList.Count;

    private Int32 leftMargin = 1;

    public Int32 LeftMargin
    {
        get => this.leftMargin;
        set
        {
            this.leftMargin = value;
            SetDrawRegion();
        }
    }

    private Int32 rightMargin = 20;

    public Int32 RightMargin
    {
        get => this.rightMargin;
        set
        {
            this.rightMargin = value;
            SetDrawRegion();
        }
    }

    private Int32 leftAxisYGuideWidth;

    public Int32 LeftAxisYGuideWidth
    {
        get => this.leftAxisYGuideWidth;
        set
        {
            Double prev = this.leftAxisYGuideWidth;
            this.leftAxisYGuideWidth = value;
            if (Math.Abs(prev - this.leftAxisYGuideWidth) > 0)
            {
                SetDrawRegion();
            }
        }
    }

    private Int32 rightAxisYGuideWidth;

    public Int32 RightAxisYGuideWidth
    {
        get => this.rightAxisYGuideWidth;
        set
        {
            Double prev = this.rightAxisYGuideWidth;
            this.rightAxisYGuideWidth = value;
            if (Math.Abs(prev - this.rightAxisYGuideWidth) > 0)
            {
                SetDrawRegion();
            }
        }
    }

    private Double width;

    public Double Width
    {
        get => this.width;
        set
        {
            Double prev = this.width;
            this.width = value;
            if (Math.Abs(prev - this.width) > 0)
            {
                SetDrawRegion();
            }
        }
    }

    public AxisXDrawRegion[] DrawRegions => this.drawRegionList.ToArray();

    public AxisXDrawRegionManager(AxisXDataManger dataManager)
    {
        this.dataManager = dataManager;
        this.dataManager.DataUpdated += (_, _) => { SetDrawRegion(); };
    }

    public void SetDisplayRange(Int32 startIndex, Int32 count)
    {
        this.StartIndex = startIndex;
        Int32 diff = this.dataManager.DataCount - this.StartIndex;
        this.DisplayCount = count > diff ? diff : count;
        SetDrawRegion();
    }

    public AxisXDrawRegion? GetDrawRegion(Double x)
    {
        foreach (var eachRegion in this.drawRegionList)
        {
            if (eachRegion.Left <= x && x < eachRegion.Left + eachRegion.Width)
            {
                return eachRegion;
            }
        }

        return null;
    }

    private void SetDrawRegion()
    {
        Double actualWidth = this.width - this.leftMargin - this.rightMargin - this.leftAxisYGuideWidth - this.rightAxisYGuideWidth;
        this.drawRegionList.Clear();
        if (actualWidth <= 0 || this.DisplayCount == 0)
        {
            this.eventSet.Raise(regionUpdatedKey, this, EventArgs.Empty);
            return;
        }

        Single eachWidth = (Single)(actualWidth / this.DisplayCount);
        Single left = this.leftMargin + this.leftAxisYGuideWidth;
        Int32 end = this.StartIndex + this.DisplayCount;
        var axisXData = this.dataManager.Data;
        for (Int32 index = this.StartIndex; index < end; index++)
        {
            this.drawRegionList.Add(new AxisXDrawRegion(axisXData[index].Value, left, left + eachWidth));
            left += eachWidth;
        }

        this.eventSet.Raise(regionUpdatedKey, this, EventArgs.Empty);
    }
}