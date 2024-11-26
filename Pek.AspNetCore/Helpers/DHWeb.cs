using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Claims;

using NewLife;

using Pek.Security.Principals;

namespace Pek.Helpers;

/// <summary>
/// Web操作
/// </summary>
public static partial class DHWeb
{
    #region 构造函数
    /// <summary>
    /// 静态构造函数
    /// </summary>
    static DHWeb()
    {
        try
        {
            Environment = Pek.Webs.HttpContext.Current.RequestServices.GetService<IWebHostEnvironment>();
            ServicePointManager.DefaultConnectionLimit = 10000000;  // 用来限制客户端请求的并发最大连接数
        }
        catch
        {
        }
    }
    #endregion

    #region Environment(宿主环境)
    /// <summary>
    /// 宿主环境
    /// </summary>
    public static IWebHostEnvironment? Environment { get; set; }
    #endregion

    #region Request(当前Http请求)
    /// <summary>
    /// 当前Http请求
    /// </summary>
    public static HttpRequest? Request => Webs.HttpContext.Current?.Request;
    #endregion

    #region HttpContext(当前Http上下文)

    /// <summary>
    /// 当前Http上下文
    /// </summary>
    public static HttpContext HttpContext => Webs.HttpContext.Current;

    #endregion

    #region User(当前用户安全主体)

    /// <summary>
    /// 当前用户安全主体
    /// </summary>
    public static ClaimsPrincipal User
    {
        get
        {
            if (HttpContext == null)
                return UnauthenticatedPrincipal.Instance;
            if (HttpContext.User is ClaimsPrincipal principal)
                return principal;
            return UnauthenticatedPrincipal.Instance;
        }
    }

    #endregion

    #region Identity(当前用户身份)

    /// <summary>
    /// 当前用户身份
    /// </summary>
    public static ClaimsIdentity Identity
    {
        get
        {
            if (User.Identity is ClaimsIdentity identity)
                return identity;
            return UnauthenticatedIdentity.Instance;
        }
    }

    #endregion

    #region 引用地址
    /// <summary>
    /// 引用地址
    /// </summary>
    public static String? RefererUrl => Request?.Headers["Referer"].FirstOrDefault();
    #endregion

    #region LocalIpAddress(本地IP)

    /// <summary>
    /// 本地IP
    /// </summary>
    public static String LocalIpAddress
    {
        get
        {
            try
            {
                var ipAddress = Webs.HttpContext.Current.Connection.LocalIpAddress;
                return IPAddress.IsLoopback(ipAddress!)
                    ? IPAddress.Loopback.ToString()
                    : ipAddress!.MapToIPv4().ToString();
            }
            catch
            {
                return IPAddress.Loopback.ToString();
            }
        }
    }

    #endregion

    #region IP(客户端IP地址)

    /// <summary>
    /// IP地址
    /// </summary>
    private static String? _ip;

    /// <summary>
    /// 设置IP地址
    /// </summary>
    /// <param name="ip">IP地址</param>
    public static void SetIp(String ip) => _ip = ip;

    /// <summary>
    /// 重置IP地址
    /// </summary>
    public static void ResetIp() => _ip = null;

    /// <summary>
    /// 客户端IP地址
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public static String IP
    {
        get
        {
            if (String.IsNullOrWhiteSpace(_ip) == false)
            {
                return _ip;
            }
            var list = new[] { "127.0.0.1", "::1" };
            var result = Webs.HttpContext.Current?.Connection?.RemoteIpAddress.SafeString();
            if (String.IsNullOrWhiteSpace(result) || list.Contains(result))
            {
                result = Runtime.Windows ? GetLanIP() : GetLanIP(NetworkInterfaceType.Ethernet);
            }
            if (result.Contains("::ffff:127.0.0.1"))
            {
                return "127.0.0.1";
            }
            return result;
        }
    }

    /// <summary>
    /// 获取局域网IP
    /// </summary>
    /// <returns></returns>
    private static String GetLanIP()
    {
        foreach (var hostAddress in Dns.GetHostAddresses(Dns.GetHostName()))
        {
            if (hostAddress.AddressFamily == AddressFamily.InterNetwork)
            {
                return hostAddress.ToString();
            }
        }
        return String.Empty;
    }

    /// <summary>
    /// 获取局域网IP。
    /// 参考地址：https://stackoverflow.com/questions/6803073/get-local-ip-address/28621250#28621250
    /// 解决OSX下获取IP地址产生"Device not configured"的问题
    /// </summary>
    /// <param name="type">网络接口类型</param>
    /// <returns></returns>
    private static String GetLanIP(NetworkInterfaceType type)
    {
        try
        {
            foreach (var item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (item.NetworkInterfaceType != type || item.OperationalStatus != OperationalStatus.Up)
                {
                    continue;
                }
                var ipProperties = item.GetIPProperties();
                if (ipProperties.GatewayAddresses.FirstOrDefault() == null)
                {
                    continue;
                }
                foreach (var ip in ipProperties.UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return ip.Address.ToString();
                    }
                }
            }
        }
        catch
        {
            return String.Empty;
        }

        return String.Empty;
    }

    #endregion

    /// <summary>
    /// 获取当前站点Url
    /// </summary>
    /// <returns></returns>
    public static String GetSiteUrl() => Request?.Scheme + "://" + Request?.Host;
}
