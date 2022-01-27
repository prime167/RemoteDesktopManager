using Tomlet.Attributes;

namespace Common;

public class Server
{
    public Server()
    {

    }

    public Server(string name, string ip, int port)
    {
        Name = name;
        IP = ip;
        Port = port;
    }


    [TomlProperty("name")]
    [TomlPrecedingComment("名称")]
    public string? Name { get; set; }

    [TomlProperty("ip")]
    [TomlInlineComment("Mqtt服务器IP地址")]
    public string IP { get; set; }

    [TomlProperty("port")]
    [TomlInlineComment("端口")]
    public int Port { get; set; }
}
