using Common;
using Tomlet.Attributes;

namespace Client;

internal class Config
{
    public Server Server { get; set; }

    public Common.Client Client { get; set; }

    public SleepTime SleepTime { get; set; }
}

[TomlDoNotInlineObject]
public class SleepTime
{
    public int Hour { get; set; }

    public int Minute { get; set; }
}