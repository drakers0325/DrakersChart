namespace DrakersChart.Axis;
public class AxisXGridManager
{
    private readonly AxisXDrawRegionManager drawRegionManager;
    public GridXGroupType GroupType { get; set; } = GridXGroupType.Month;

    public XGridInfo[] Infos { get; private set; } = [];

    public Boolean UseCenterGrid { get; set; } = false;

    public AxisXGridManager(AxisXDrawRegionManager drawRegionManager)
    {
        this.drawRegionManager = drawRegionManager;
        this.drawRegionManager.DrawRegionsUpdated += (_, _) => { UpdateInfos(); };
    }

    public void UpdateInfos()
    {
        switch (this.GroupType)
        {
            case GridXGroupType.Value:
                break;
            case GridXGroupType.Month:
                this.Infos = CreateMonthGridInfo();
                break;
            case GridXGroupType.Year:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private XGridInfo[] CreateMonthGridInfo()
    {
        var groups = this.drawRegionManager.DrawRegions
            .GroupBy(r =>
            {
                var dt = DateTime.FromBinary(r.X);
                return (dt.Year, dt.Month);
            })
            .OrderBy(g => g.Key.Year)
            .ThenBy(g => g.Key.Month);

        var gridList = new List<XGridInfo>();
        foreach (var eachGroup in groups)
        {
            var values = eachGroup.ToArray();
            Single coordinate = this.UseCenterGrid ? values[0].Center : values[0].Left;
            Single width = this.UseCenterGrid ?
                values[^1].Center - values[0].Center :
                values[^1].Right - values[0].Left;
            var x = DateTime.FromBinary(values[0].X);
            String text = x.Month == 1 ? $"{x.Year}/{x.Month:00}" : $"{x.Month:00}";
            var info = new XGridInfo(values[0].X, coordinate, width, text);
            gridList.Add(info);
        }

        if (gridList.Count <= 0 || gridList.Count == 0 || gridList.Average(g => g.Width) > 70)
        {
            return gridList.ToArray();
        }

        var origin = gridList.ToArray();
        gridList.Clear();
        var sumList = new List<XGridInfo>();
        foreach (var eachInfo in origin)
        {
            var dt = DateTime.FromBinary(eachInfo.X);
            if (dt.Month == 1 && sumList.Count > 0)
            {
                gridList.Add(CreateMergedXGridInfo(sumList));
                sumList.Clear();
            }

            sumList.Add(eachInfo);
        }

        if (sumList.Count > 0)
        {
            gridList.Add(CreateMergedXGridInfo(sumList));
        }

        return gridList.ToArray();
    }

    private static XGridInfo CreateMergedXGridInfo(List<XGridInfo> sumList)
    {
        var dt = DateTime.FromBinary(sumList[0].X);
        Int64 x = sumList[0].X;
        Single coordinate = sumList[0].Coordinate;
        Single width = sumList.Sum(i => i.Width);
        String text = dt.Year.ToString();
        return new XGridInfo(x, coordinate, width, text);
    }
}