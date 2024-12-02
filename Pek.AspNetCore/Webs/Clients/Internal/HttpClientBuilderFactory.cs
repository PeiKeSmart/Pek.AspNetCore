using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace Pek.Webs.Clients.Internal;

/// <summary>
/// HttpClient 生成工厂
/// </summary>
internal static class HttpClientBuilderFactory {
    /// <summary>
    /// HttpClient 字典
    /// </summary>
    private static readonly ConcurrentDictionary<String, HttpClient> _httpClients =
        new();

    /// <summary>
    /// 域名正则表达式
    /// </summary>
    private static readonly Regex _domainRegex =
        new(@"(http|https)://(?<domain>[^(:|/]*)", RegexOptions.IgnoreCase);

    /// <summary>
    /// 创建Http客户端
    /// </summary>
    /// <param name="url">请求地址</param>
    /// <param name="timeout">超时时间</param>
    public static HttpClient CreateClient(String url, TimeSpan timeout)
    {
        var domain = GetDomainByUrl(url);
        if (_httpClients.TryGetValue(domain, out var value))
            return value;
        var httpClient = Create(timeout);
        _httpClients[domain] = httpClient;
        return httpClient;
    }

    /// <summary>
    /// 通过Url地址获取域名
    /// </summary>
    /// <param name="url">Url地址</param>
    private static String GetDomainByUrl(String url) => _domainRegex.Match(url).Value;

    /// <summary>
    /// 创建Http客户端
    /// </summary>
    private static HttpClient Create(TimeSpan timeout)
    {
        var httpClient = new HttpClient(new HttpClientHandler()
        {
            UseProxy = false,
        })
        {
            Timeout = timeout
        };
        //httpClient.DefaultRequestHeaders.Connection.Add("keep-alive");
        return httpClient;
    }
}
