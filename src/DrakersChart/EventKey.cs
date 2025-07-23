namespace DrakersChart;
public class EventKey(String eventName)
{
    public String EventName { get; private set; } = eventName;

    public EventKey() : this(String.Empty) { }
}