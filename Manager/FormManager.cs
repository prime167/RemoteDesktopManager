using System.Text;
using CliWrap;
using MQTTnet;
using MQTTnet.Client;

using MQTTnet.Formatter;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using MQTTnet.Server;
using Tomlet;

namespace Manager;

public partial class FormManager : Form
{
    private MqttServer _mqttServer;
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
                BtnSleep_Click(this, EventArgs.Empty);
                //BtnSleepSelf_Click(this, EventArgs.Empty);
            }
        }
    }

    private void MqttServer()
    {
        if (_mqttServer is not null)
        {
            return;
        }

        MqttServerOptionsBuilder builder = new MqttServerOptionsBuilder();
        var options =
            builder
                .WithDefaultEndpoint()
                .WithConnectionBacklog(1000)
                .WithDefaultEndpointPort(_config.Server.Port).Build();

        _mqttServer = new MqttFactory().CreateMqttServer(options);
        _mqttServer.ClientConnectedAsync += async e =>
        {
            listBox1.BeginInvoke(_updateListBoxAction, $"{DateTime.Now} Client {e.ClientId} connected");
            label1.BeginInvoke(() =>
            {
                if (e.ClientId == "16")
                {
                    label1.BackColor = Color.Green;
                }
            });
        };

        _mqttServer.ClientDisconnectedAsync += async e =>
        {
            listBox1.BeginInvoke(_updateListBoxAction, $"{DateTime.Now} Client {e.ClientId} disconnected");
            label1.BeginInvoke(() =>
            {
                if (e.ClientId == "16")
                {
                    label1.BackColor = Color.Gray;
                }
            });

        };

        _mqttServer.StartAsync().Wait();
    }

    private void MqttClient()
    {
        //try
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

            _mqttClient.ApplicationMessageReceivedAsync += async e =>
            {
                DeakMessage(e);
            };

            _mqttClient.ConnectedAsync += async e =>
            {
                //listBox1.BeginInvoke(_updateListBoxAction,
                //    $"{DateTime.Now} Client is Connected:  IsSessionPresent:{e.ConnectResult}");
            };

            _mqttClient.DisconnectedAsync += async e =>
            {
                //listBox1.BeginInvoke(_updateListBoxAction,
                //    $"{DateTime.Now} Client is DisConnected ClientWasConnected:{e.ClientWasConnected}");
            };

            _mqttClient.ConnectAsync(options).Wait();
            _mqttClient.SubscribeAsync(
               new MqttTopicFilter
               {
                   Topic = "status/#",
                   QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce
               }).Wait();
        }
        //catch (Exception ex)
        {
        //    throw;
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

        _mqttClient.PublishAsync(msg);
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

    private void BtnSleepSelf_Click(object sender, EventArgs e)
    {
        Cli.Wrap("cmd").WithArguments($@"/C shutdown /h ").ExecuteAsync();

    }
}
