namespace DrakersChart.Axis;
public readonly struct AxisXDrawRegion(Int64 x, Single left, Single right)
{
    public Int64 X { get; } = x;
    public Single Left { get; } = left;
    public Single Right { get; } = right;

    public Single Width => this.Right - this.Left;
    public Single Center => this.Left + (this.Width / 2);
}