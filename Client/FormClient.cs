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

namespace RemoteDesktopClient;

public partial class FormClient : Form
{
    private IMqttClient _mqttClient;
    private Config _config;
    private Timer _timer;
    private int _failCount;
    private Logger _logger = LogManager.GetCurrentClassLogger();
    private int _totalOkCount = 0;
    private int _okCount = 0;
    private NetworkTime NetworkTime = new NetworkTime();
    private int _lastState;
    private int _networkFailCausedSleepCount;
    private const int Interval = 3000;

    public FormClient()
    {
        InitializeComponent();
    }

    private void FormClient_Load(object sender, EventArgs e)
    {
        _logger.Debug("Init...");
        var str = File.ReadAllText("config.toml");
        _config = TomletMain.To<Config>(str);
        _timer = new Timer(CallBack, null, Interval, Interval);
        MqttClient();
        if (_config.SleepTime.Hour >= 0)
        {
            LblHibernateTime.Text = $"{_config.SleepTime.Hour}:{_config.SleepTime.Minute}";
        }
    }

    private void CallBack(object? state)
    {
        var sysTime = DateTime.Now;
        var nt = NetworkTime.GetNetworkTime();
        if (nt.Year < 1970)
        {
            _failCount++;
            _okCount = 0;
            this.BeginInvoke(() =>
            {
                LblNetworkConnState.Text = "x";
                LblNetworkConnState.ForeColor = Color.Red;
            });

            if (_lastState == 11 || _lastState == 12)
            {
                _lastState = 0;
                _timer.Change(Interval, Interval);
            }
        }
        else
        {
            this.BeginInvoke(() =>
            { 
                LblNetworkConnState.Text = "√";
                LblNetworkConnState.ForeColor = Color.Green;
            });

            if (_totalOkCount == 0)
            {
                if (_lastState == 0)
                {
                    _logger.Info($"软件启动, 网络时间:{nt}, 本机时间:{sysTime}");
                }
                else if (_lastState > 10 && _lastState < 20)
                {
                    _timer.Change(Interval, Interval);
                    _lastState = 0;
                    _logger.Info($"软件恢复, 网络时间:{nt}, 本机时间:{sysTime}");
                }
            }
            else if (_failCount > 0)
            {
                _logger.Info($"网络失败 {_failCount} 次后恢复, 网络时间:{nt}, 本机时间:{sysTime}");
            }

            _failCount = 0;
            _okCount++;
            _totalOkCount++;
        }

        if (_failCount == 0)
        {
            TimeReachedSleep(sysTime, nt);
        }
        else
        {
            NetworkFailSleep();
        }
    }

    private void TimeReachedSleep(DateTime sysTime, DateTime dt)
    {
        var hour = _config.SleepTime.Hour;
        var minute = _config.SleepTime.Minute;
        if (dt.Hour == hour && dt.Minute >= minute && dt.Minute < minute + 1)
        {
            if (_lastState != 12)
            {
                _logger.Info($"到达设定休眠时间，进入休眠. 网络时间:{dt}, 本机时间:{sysTime}");
                _networkFailCausedSleepCount = 0;
                Sleep(12);
            }
        }
    }

    private void NetworkFailSleep()
    {
        // 达到一定失败次数才休眠，防止循环休眠
        int minute = _networkFailCausedSleepCount + 1;
        var times = (1000 * 60 / Interval) * minute + 1;
        if (_failCount >= times) // 一直连不上
        {
            _networkFailCausedSleepCount++;
            _logger.Info($"网络失败次数达到最大值({times - 1}次)，自动休眠(第{_networkFailCausedSleepCount}次)");
            Sleep(11);
        }
    }

    private void Sleep(int state)
    {
        _totalOkCount = 0;
        _okCount = 0;
        _failCount = 0;
        _lastState = state;
        TimeSpan interval = TimeSpan.FromMinutes(5);
        _timer.Change(interval, interval);
        Sleep(true);
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
                this.BeginInvoke(() =>
                {
                    LblServerConnState.Text = "√";
                    LblServerConnState.ForeColor = Color.Green;
                });
            });

            _mqttClient.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(e =>
            {
                this.BeginInvoke(() =>
                {
                    LblServerConnState.Text = "x";
                    LblServerConnState.ForeColor = Color.Red;
                });

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
        //listBox1.BeginInvoke(
        //                        _updateListBoxAction,
        //                        $"{DateTime.Now} ClientID:{e.ClientId} | TOPIC:{topic} | Payload:{payload} | QoS:{e.ApplicationMessage.QualityOfServiceLevel} | Retain:{e.ApplicationMessage.Retain}"
        //                        );
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

    private void Shutdown(bool real)
    {
        if (!real)
        {
            _logger.Debug("模拟关机");
        }
        else
        {
            Cli.Wrap("cmd").WithArguments($@"/C shutdown -s -t 0").ExecuteAsync();
        }
    }

    private void Sleep(bool real)
    {
        if (!real)
        {
            _logger.Debug("模拟休眠");
        }
        else
        {
            _logger.Info($"服务器，进入休眠");
            Cli.Wrap("cmd").WithArguments($@"/C shutdown /h ").ExecuteAsync();
        }
    }
}
