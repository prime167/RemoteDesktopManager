using NLog;
using System.Diagnostics;
using System.Net.Http.Headers;
using Tomlet;

namespace Client;

public class NetworkTime
{
    private HttpClient httpClient;
    private string[] _urls;

    private int _urlIndex;

    private Logger _logger = LogManager.GetCurrentClassLogger();

    public NetworkTime()
    {
        httpClient = new HttpClient(new HttpClientHandler { UseProxy = false });
        httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { NoCache = true };
        httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (compatible, MSIE 11, Windows NT 6.3; Trident/7.0; rv:11.0) like Gecko");
        httpClient.Timeout = TimeSpan.FromSeconds(2);
        var str = File.ReadAllText("urls.toml");
        _urls = TomletMain.To<UrlConfig>(str).Urls;
    }

    /// <summary>
    /// 获取网络日期时间
    /// </summary>
    /// <returns></returns>
    public DateTime GetNetworkTime()
    {
        string url="";
        try
        {
            url = GetNextUrl();
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
            _logger.Trace(url+" "+ex.Message);
            return DateTime.MinValue;
        }
        finally
        {

        }
    }

    private string GetNextUrl()
    {
        var url = _urls[_urlIndex++ % _urls.Count()];
        return url;
    }
}
