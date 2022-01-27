using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Formatter;
using System.Text;
using MQTTnet.Protocol;
using CliWrap;
using Client;
using Tomlet;
using Timer = System.Threading.Timer;
using System.Net.Http.Headers;
using NLog;

namespace RemoteDesktopClient;

public partial class FormClient : Form
{
    private IMqttClient _mqttClient;
    private Action<string> _updateListBoxAction;
    private Config _config;
    private Timer _timer;
    private int _failCount;
    private Logger _logger = LogManager.GetCurrentClassLogger();
    private int _totalOkCount = 0;
    private int _okCount = 0;
    private DateTime _upTime;// 系统开机，休眠恢复时间
    private int _failCausedSleepCount;
    private NetworkTime NetworkTime = new NetworkTime();

    public FormClient()
    {
        InitializeComponent();
    }

    private void FormClient_Load(object sender, EventArgs e)
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

        _timer = new Timer(CallBack, null, 0, 1000);
        MqttClient();
    }

    private void CallBack(object? state)
    {
        var sysTime = DateTime.Now;
        var dt = NetworkTime.GetNetworkTime();
        if (dt.Year < 1970)
        {
            _failCount++;
            _okCount = 0;
            _logger.Warn($"network fail {_failCount} 网络时间:{dt}, 本机时间:{sysTime}");
        }
        else
        {
            if (_totalOkCount == 0)
            {
                _upTime = dt;
                _logger.Info($"系统启动（恢复）, 网络时间:{dt}, 本机时间:{sysTime}");
            }

            if (_failCount > 0 && _okCount == 0)
            {
                _logger.Info($"网络恢复, 网络时间:{dt}, 本机时间:{sysTime}");
            }

            _failCount = 0;
            _okCount++;
            _totalOkCount++;
        }

        // 之前连接成功一段时间，达到一定失败次数，且之前没有因此休眠过才休眠，防止循环休眠
        if (_totalOkCount > 300 && _failCount >= 30 && _failCausedSleepCount == 0)
        {
            _logger.Info($"network failed too often,({_failCount}), sleep");
            _totalOkCount = 0;
            _okCount = 0;
            _failCausedSleepCount++;
            Sleep(false);
        }
        else if (dt.Hour == 17 && dt.Minute >= 20 && dt.Minute <= 30)
        {
            _logger.Info($"network ok and time reached, sleep. 网络时间:{dt}, 本机时间:{sysTime}");
            _totalOkCount = 0;
            _okCount = 0;
            _failCausedSleepCount = 0;
            Sleep(true);
        }
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
            options.WillMessage = new MqttApplicationMessage
            {
                Topic = "status/m16",
                Payload = Encoding.UTF8.GetBytes("disconnected"),
                QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce,
                Retain = false
            };

            if (null != _mqttClient)
            {
                _mqttClient.DisconnectAsync();
                _mqttClient = null;
            }

            _mqttClient = new MqttFactory().CreateMqttClient();

            _mqttClient.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(e =>
            {
                DealMessage(e);
            });

            _mqttClient.ConnectedHandler = new MqttClientConnectedHandlerDelegate(e =>
            {
                listBox1.BeginInvoke(_updateListBoxAction,
                    $"{DateTime.Now} Client is Connected");
            });

            _mqttClient.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(e =>
            {
                listBox1.BeginInvoke(_updateListBoxAction,
                    $"{DateTime.Now} Client is DisConnected ClientWasConnected:{e.ClientWasConnected}");

                Thread.Sleep(1000);
                _mqttClient.ConnectAsync(options).Wait();
                _mqttClient.SubscribeAsync(
                   new MqttTopicFilter
                   {
                       Topic = "command/m16",
                       QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce
                   }).Wait();

                for (int i = 0; i < 10; i++)
                {
                    var msg = new MqttApplicationMessage
                    {
                        Topic = "status/m16",
                        Payload = Encoding.UTF8.GetBytes("connected"),
                        QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce,
                        Retain = false
                    };

                    if (_mqttClient is not null)
                    {
                        _mqttClient.PublishAsync(msg).Wait();
                    }

                    Thread.Sleep(100);
                }
            });

            _mqttClient.ConnectAsync(options).Wait();
            _mqttClient.SubscribeAsync(
               new MqttTopicFilter
               {
                   Topic = "command/m16",
                   QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce
               }).Wait();

            var msg = new MqttApplicationMessage
            {
                Topic = "status/m16",
                Payload = Encoding.UTF8.GetBytes("connected"),
                QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce,
                Retain = false
            };

            if (_mqttClient is not null)
            {
                _mqttClient.PublishAsync(msg);
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex.Message);
        }
    }

    private void DealMessage(MqttApplicationMessageReceivedEventArgs e)
    {
        var topic = e.ApplicationMessage.Topic;
        var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
        listBox1.BeginInvoke(
                                _updateListBoxAction,
                                $"{DateTime.Now} ClientID:{e.ClientId} | TOPIC:{topic} | Payload:{payload} | QoS:{e.ApplicationMessage.QualityOfServiceLevel} | Retain:{e.ApplicationMessage.Retain}"
                                );
        if (topic == "command/m16")
        {
            if (payload == "shutdown")
            {
                Shutdown(true);
            }
            else if (payload == "sleep")
            {
                Sleep(true);
            }
        }
    }

    private static void Shutdown(bool real)
    {
        if (!real)
        {
            Cli.Wrap("cmd").WithArguments($@"/C code ").ExecuteAsync();
        }
        else
        {
            Cli.Wrap("cmd").WithArguments($@"/C shutdown -s -t 0").ExecuteAsync();
        }
    }

    private static void Sleep(bool real)
    {
        if (!real)
        {
            Cli.Wrap("cmd").WithArguments($@"/C code ").ExecuteAsync();
        }
        else
        {
            Cli.Wrap("cmd").WithArguments($@"/C shutdown /h ").ExecuteAsync();
        }
    }
}
