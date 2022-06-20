using Tomlet.Attributes;

namespace Common;

public class Client
{
    public Client()
    {

    }

    public Client(string name)
    {
        Name = name;
    }


    [TomlProperty("name")]
    [TomlPrecedingComment("名称")]
    public string? Name { get; set; }
}
