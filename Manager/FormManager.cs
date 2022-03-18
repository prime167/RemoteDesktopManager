using MQTTnet.Client.Receiving;
using MQTTnet.Server;
using MQTTnet;
using MQTTnet.Protocol;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Formatter;
using System.Text;
using MQTTnet.Client;
using Manager;
using Tomlet;
using CliWrap;

namespace RemoteDesktopManager;

public partial class FormManager : Form
{
    private IMqttServer _mqttServer;
    private IMqttClient _mqttClient;
    private Action<string> _updateListBoxAction;
    private Config _config;

    public string[] Args { get; set; }

    public FormManager()
    {
        InitializeComponent();
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        _updateListBoxAction = (s) =>
        {
            listBox1.BeginUpdate();
            listBox1.Items.Add(s);
            if (listBox1.Items.Count > 100)
            {
                listBox1.Items.RemoveAt(0);
            }

            listBox1.TopIndex = listBox1.Items.Count - 1;
            listBox1.EndUpdate();
        };

        var str = File.ReadAllText("config.toml");
        _config = TomletMain.To<Config>(str);

        MqttServer();
        Thread.Sleep(1000);
        MqttClient();

        if (Args.Length > 0)
        {
            var arg = Args[0];
            if (arg == "both")
            {
                BtnSleep_Click(null, null);
                BtnShutDownSelf_Click(null, null);
            }
        }
    }

    private void MqttServer()
    {
        if (_mqttServer is not null)
        {
            return;
        }

        var optionBuilder =
            new MqttServerOptionsBuilder()
                .WithConnectionBacklog(1000)
                .WithDefaultEndpointPort(_config.Server.Port);

        _mqttServer = new MqttFactory().CreateMqttServer();
        _mqttServer.ClientConnectedHandler = new MqttServerClientConnectedHandlerDelegate(e =>
        {
            listBox1.BeginInvoke(_updateListBoxAction, $"{DateTime.Now} Client {e.ClientId} connected");
            label1.BeginInvoke(() =>
            {
                label1.BackColor = Color.Green;
            });
        });

        _mqttServer.ClientDisconnectedHandler = new MqttServerClientDisconnectedHandlerDelegate(e =>
        {
            listBox1.BeginInvoke(_updateListBoxAction, $"{DateTime.Now} Client {e.ClientId} disconnected");
            label1.BeginInvoke(() =>
            {
                label1.BackColor = Color.Gray;
            });
        });

        _mqttServer.StartAsync(optionBuilder.Build()).Wait();
    }

    private void MqttClient()
    {
        try
        {
            var options = new MqttClientOptions
            {
                ClientId = _config.Server.Name,
                ProtocolVersion = MqttProtocolVersion.V500
            };

            options.ChannelOptions = new MqttClientTcpOptions
            {
                Server = _config.Server.IP,
                Port = _config.Server.Port
            };

            options.CleanSession = true;
            options.KeepAlivePeriod = TimeSpan.FromSeconds(100.5);

            if (null != _mqttClient)
            {
                _mqttClient.DisconnectAsync();
                _mqttClient = null;
            }

            _mqttClient = new MqttFactory().CreateMqttClient();

            _mqttClient.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(e =>
            {
                DeakMessage(e);
            });

            _mqttClient.ConnectedHandler = new MqttClientConnectedHandlerDelegate(e =>
            {
                //listBox1.BeginInvoke(_updateListBoxAction,
                //    $"{DateTime.Now} Client is Connected:  IsSessionPresent:{e.ConnectResult}");
            });

            _mqttClient.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(e =>
            {
                //listBox1.BeginInvoke(_updateListBoxAction,
                //    $"{DateTime.Now} Client is DisConnected ClientWasConnected:{e.ClientWasConnected}");
            });

            _mqttClient.ConnectAsync(options).Wait();
            _mqttClient.SubscribeAsync(
               new MqttTopicFilter
               {
                   Topic = "status/#",
                   QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce
               }).Wait();
        }
        catch (Exception ex)
        {
            throw;
        }

        void DeakMessage(MqttApplicationMessageReceivedEventArgs e)
        {
            var topic = e.ApplicationMessage.Topic;
            var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

            label1.BeginInvoke(() => 
            {
                if (topic.StartsWith("status"))
                {
                    if (topic.Contains("m16"))
                    {
                        if (payload == "connected")
                        {
                            label1.BackColor = Color.Green;
                        }
                        else if (payload == "disconnected")
                        {
                            label1.BackColor = Color.Gray;
                        }
                    }
                }
            });
        }
    }

    private void BtnSleep_Click(object sender, EventArgs e)
    {
        var msg = new MqttApplicationMessage
        {
            Topic = "command/m16",
            Payload = Encoding.UTF8.GetBytes("sleep"),
            QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce,
            Retain = false
        };

        if (_mqttClient is not null)
        {
            _mqttClient.PublishAsync(msg);
        }
    }

    private void BtnShutDown_Click(object sender, EventArgs e)
    {
        var msg = new MqttApplicationMessage
        {
            Topic = "command/m16",
            Payload = Encoding.UTF8.GetBytes("shutdown"),
            QualityOfServiceLevel= MqttQualityOfServiceLevel.AtLeastOnce,
            Retain = false
        };

        if (_mqttClient is not null)
        {
            _mqttClient.PublishAsync(msg);
        }
    }

    private void BtnShutDownSelf_Click(object sender, EventArgs e)
    {
        Cli.Wrap("cmd").WithArguments($@"/C shutdown -s -t 0 ").ExecuteAsync();
        //Cli.Wrap("cmd").WithArguments($@"/C code ").ExecuteAsync();
    }
}
