using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Tomlet;

namespace Client;

public class NetworkTime
{
    private HttpClient httpClient;
    private string[] _urls;

    private int _urlIndex;

    private Random rnd = Random.Shared;
    private int lastRandom;
    private Logger _logger = LogManager.GetCurrentClassLogger();

    public NetworkTime()
    {
        httpClient = new HttpClient(new HttpClientHandler { UseProxy = false });
        httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible, MSIE 11, Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko");
        httpClient.Timeout = TimeSpan.FromSeconds(1);
        var str = File.ReadAllText("urls.toml");
        _urls = TomletMain.To<UrlConfig>(str).Urls;
    }

    /// <summary>
    /// 获取网络日期时间
    /// </summary>
    /// <returns></returns>
    public DateTime GetNetworkTime()
    {
        try
        {
            var url = GetNextUrl();
            var sw = Stopwatch.StartNew();
            var response = httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Head, url)).Result;
            response.EnsureSuccessStatusCode();
            sw.Stop();
            var r = response.Headers.TryGetValues("Date", out var dateTime);
            var dd = dateTime.ToArray()[0];
            var dt = DateTime.Parse(dd);
            _logger.Trace($"{url,-30} {dt}");
            return dt;
        }
        catch (Exception ex)
        {
            _logger.Debug(ex.Message);
            return DateTime.MinValue;
        }
        finally
        {

        }
    }

    private string GetNextUrl()
    {
        //do
        //{
        //    _urlIndex = rnd.Next(0, _urls.Count());
        //} while (_urlIndex == lastRandom);

        //lastRandom = _urlIndex;

        var url = _urls[_urlIndex++ % _urls.Count()];
        return url;
    }
}
