using System.Text;
using CliWrap;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Formatter;
using MQTTnet.Packets;
using MQTTnet.Protocol;
using NLog;
using Tomlet;
using Timer = System.Threading.Timer;

namespace Client;

public partial class FormClient : Form
{
    private IMqttClient _mqttClient;
    private Config _config;
    private Timer _timer;
    private int _failCount;
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();
    private int _totalOkCount = 0;
    private int _okCount = 0;
    private readonly NetworkTime _networkTime = new NetworkTime();
    private int _lastState;
    private int _networkFailCausedSleepCount;
    private const int Interval = 3000;

    public FormClient()
    {
        InitializeComponent();
    }

    private async void FormClient_Load(object sender, EventArgs e)
    {
        _logger.Debug("Init...");
        var str = await File.ReadAllTextAsync("config.toml");
        _config = TomletMain.To<Config>(str);
        if (_config.SleepTime.Hour >= 0)
        {
            LblHibernateTime.Text = $"{_config.SleepTime.Hour}:{_config.SleepTime.Minute}";
        }

        _timer = new Timer(CallBack, null, Interval, Interval);
        await MqttClient();
    }

    private void CallBack(object state)
    {
        var sysTime = DateTime.Now;
        var nt = _networkTime.GetNetworkTime();
        if (nt.Year < 1970)
        {
            _failCount++;
            _okCount = 0;
            this.BeginInvoke(() =>
            {
                LblNetworkConnState.Text = "x";
                LblNetworkConnState.ForeColor = Color.Red;
            });

            if (_lastState == 11 || _lastState == 12 || _lastState == 13)
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
                else if (_lastState is > 10 and < 20)
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

    private async Task MqttClient()
    {
        try
        {
            var options = new MqttClientOptions
            {
                ClientId = _config.Client.Name,
                ProtocolVersion = MqttProtocolVersion.V500
            };

            options.ChannelOptions = new MqttClientTcpOptions
            {
                Server = _config.Server.IP,
                Port = _config.Server.Port
            };

            options.CleanSession = true;
            options.KeepAlivePeriod = TimeSpan.FromSeconds(100.5);
            options.WillTopic = "status/m16";
            options.WillRetain = false;
            options.WillQualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce;
            options.WillPayload = Encoding.UTF8.GetBytes("disconnected");

            if (null != _mqttClient)
            {
                await _mqttClient.DisconnectAsync();
                _mqttClient = null;
            }

            _mqttClient = new MqttFactory().CreateMqttClient();

            _mqttClient.ApplicationMessageReceivedAsync += async e =>
            {
                DealMessage(e);
            };

            _mqttClient.ConnectedAsync += async e =>
            {
                this.BeginInvoke(() =>
                {
                    LblServerConnState.Text = "√";
                    LblServerConnState.ForeColor = Color.Green;
                });
            };

            _mqttClient.DisconnectedAsync += async e =>
            {
                this.BeginInvoke(() =>
                {
                    LblServerConnState.Text = "x";
                    LblServerConnState.ForeColor = Color.Red;
                });

                await Task.Delay(1000);
                try
                {
                    await _mqttClient.ConnectAsync(options).WaitAsync(TimeSpan.FromMilliseconds(300));
                }
                catch (Exception ex)
                {
                    //_logger.Error("无法重新连接到服务器:" + ex.Message);
                }

                await _mqttClient.SubscribeAsync(
                   new MqttTopicFilter
                   {
                       Topic = "command/m16",
                       QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce
                   });

                var msg = new MqttApplicationMessage
                {
                    Topic = "status/m16",
                    Payload = Encoding.UTF8.GetBytes("connected"),
                    QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce,
                    Retain = false
                };

                if (_mqttClient is not null)
                {
                    await _mqttClient.PublishAsync(msg);
                }
            };

            try
            {
                await _mqttClient.ConnectAsync(options).WaitAsync(TimeSpan.FromMilliseconds(300));
            }
            catch (Exception ex)
            {
                _logger.Error("无法连接到服务器:"+ex.Message);
            }
            await _mqttClient.SubscribeAsync(
               new MqttTopicFilter
               {
                   Topic = "command/m16",
                   QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce
               });

            var msg = new MqttApplicationMessage
            {
                Topic = "status/m16",
                Payload = Encoding.UTF8.GetBytes("connected"),
                QualityOfServiceLevel = MqttQualityOfServiceLevel.AtLeastOnce,
                Retain = false
            };

            if (_mqttClient is not null)
            {
                await _mqttClient.PublishAsync(msg);
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
                _okCount = 0;
                _logger.Info($"收到服务器休眠领命，进入休眠");
                //Sleep(13);
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
            Cli.Wrap("cmd").WithArguments($@"/C shutdown /h ").ExecuteAsync();
        }
    }
}
