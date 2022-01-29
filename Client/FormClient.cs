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
using NLog;
using System.Diagnostics;

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
    private DateTime _upTime;// ϵͳ���������߻ָ�ʱ��
    private NetworkTime NetworkTime = new NetworkTime();
    private Stopwatch _stopwatch;
    private int _lastState;
    private DateTime _lastSleepTime;

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
        _stopwatch = Stopwatch.StartNew();
        _timer = new Timer(CallBack, null, 1000, 3000);
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
            _logger.Warn($"network fail {_failCount} ����ʱ��:{dt}, ����ʱ��:{sysTime}");
        }
        else
        {
            if (_totalOkCount == 0)
            {
                if (_lastState == 0)
                {
                    _upTime = dt;
                    _logger.Info($"�������, ����ʱ��:{dt}, ����ʱ��:{sysTime}");
                }
                else if (_lastState > 10 && _lastState < 20)
                {
                    _timer.Change(1000, 1000);
                    _lastState = 0;
                    _upTime = dt;
                    _logger.Info($"����ָ�, ����ʱ��:{dt}, ����ʱ��:{sysTime}");
                }
            }
            else if (_failCount > 0)
            {
                _logger.Info($"����ָ�, ����ʱ��:{dt}, ����ʱ��:{sysTime}");
            }

            _failCount = 0;
            _okCount++;
            _totalOkCount++;
        }

        // ֮ǰ���ӳɹ�һ��ʱ�䣬�ﵽһ��ʧ�ܴ�������֮ǰû��������߹������ߣ���ֹѭ������
        if (_lastState != 11 && _totalOkCount > 300 && _failCount >= 30)
        {
            _logger.Info($"network failed too often,({_failCount}), sleep");
            _totalOkCount = 0;
            _okCount = 0;
            _lastState = 11;
            _lastSleepTime = dt;
            //Sleep(true);
        }

        var hour = _config.SleepTime.Hour;
        var minute = _config.SleepTime.Minute;
        if (dt.Hour == hour && dt.Minute >= minute && dt.Minute < minute + 1)
        {
            if (_lastState != 12)
            {
                _logger.Info($"network ok and time reached, sleep. ����ʱ��:{dt}, ����ʱ��:{sysTime}");
                _okCount = 0;
                _totalOkCount = 0;
                _lastState = 12;
                _lastSleepTime = dt;
                _timer.Change(1000 * 60 * 2, 1000 * 60 * 2);
                Sleep(true);
            }
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
                _lastState = 13;
                _okCount = 0;
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
