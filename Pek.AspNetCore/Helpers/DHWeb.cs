using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Claims;
using System.Text;
using System.Web;

using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Net.Http.Headers;

using NewLife;
using NewLife.Log;

using Pek.IO;
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

            ServicePointManager.ReusePort = true;  // 可以让不同的HTTP请求重用相同的本地端口。

            ServicePointManager.Expect100Continue = false;  // 询问服务器是否愿意接受数据,禁用此选项可能会提高性能。
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

    #region Response(当前Http响应)

    /// <summary>
    /// 当前Http响应
    /// </summary>
    public static HttpResponse? Response => Pek.Webs.HttpContext.Current?.Response;

    #endregion

    #region HttpContext(当前Http上下文)

    /// <summary>
    /// 当前Http上下文
    /// </summary>
    public static HttpContext HttpContext => Webs.HttpContext.Current;

    #endregion

    #region RequestType(请求类型)

    /// <summary>
    /// 请求类型
    /// </summary>
    public static String? RequestType => Pek.Webs.HttpContext.Current?.Request?.Method;

    #endregion

    #region Form(表单)

    /// <summary>
    /// Form表单
    /// </summary>
    public static IFormCollection? Form => Pek.Webs.HttpContext.Current?.Request?.Form;

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

    #region AccessToken(获取访问令牌)

    /// <summary>
    /// 获取访问令牌
    /// </summary>
    public static String? AccessToken
    {
        get
        {
            var authorization = Request?.Headers.Authorization;
            if (String.IsNullOrWhiteSpace(authorization))
                return null;
            var list = authorization.SafeString().Split(' ');
            if (list.Length == 2)
                return list[1];
            return null;
        }
    }

    #endregion

    #region Body(请求正文)

    /// <summary>
    /// 请求正文
    /// </summary>
    public static String Body
    {
        get
        {
            Request?.EnableBuffering();
            return FileUtil.ToString(Request?.Body, isCloseStream: false);
        }
    }

    /// <summary>
    /// 获取请求正文
    /// </summary>
    /// <returns></returns>
    public static async Task<String> GetBodyAsync()
    {
        Request?.EnableBuffering();
        return await FileUtil.ToStringAsync(Request?.Body, isCloseStream: false).ConfigureAwait(false);
    }

    #endregion

    #region UrlEncode(Url编码)

    /// <summary>
    /// Url编码
    /// </summary>
    /// <param name="url">url</param>
    /// <param name="isUpper">编码字符是否转成大写，范例："http://"转成"http%3A%2F%2F"</param>
    /// <returns></returns>
    public static String UrlEncode(this String url, Boolean isUpper = false) => UrlEncode(url, Encoding.UTF8, isUpper);

    /// <summary>
    /// Url编码
    /// </summary>
    /// <param name="url">url</param>
    /// <param name="encoding">字符编码</param>
    /// <param name="isUpper">编码字符是否转成大写，范例："http://"转成"http%3A%2F%2F"</param>
    /// <returns></returns>
    public static String UrlEncode(this String url, String encoding, Boolean isUpper = false)
    {
        encoding = String.IsNullOrWhiteSpace(encoding) ? "UTF-8" : encoding;
        return UrlEncode(url, Encoding.GetEncoding(encoding), isUpper);
    }

    /// <summary>
    /// Url编码
    /// </summary>
    /// <param name="url">url</param>
    /// <param name="encoding">字符编码</param>
    /// <param name="isUpper">编码字符是否转成大写，范例："http://"转成"http%3A%2F%2F"</param>
    /// <returns></returns>
    public static String UrlEncode(this String url, Encoding encoding, Boolean isUpper = false)
    {
        var result = HttpUtility.UrlEncode(url, encoding);
        if (isUpper == false)
        {
            return result;
        }

        return GetUpperEncode(result);
    }

    /// <summary>
    /// 获取大写编码字符串
    /// </summary>
    /// <param name="encode">编码字符串</param>
    /// <returns></returns>
    private static String GetUpperEncode(String encode)
    {
        var result = new StringBuilder();
        var index = Int32.MinValue;
        for (var i = 0; i < encode.Length; i++)
        {
            var character = encode[i].ToString();
            if (character == "%")
            {
                index = i;
            }

            if (i - index == 1 || i - index == 2)
            {
                character = character.ToUpper();
            }

            result.Append(character);
        }

        return result.ToString();
    }

    #endregion

    #region UrlDecode(Url解码)

    /// <summary>
    /// Url解码
    /// </summary>
    /// <param name="url">url</param>
    /// <returns></returns>
    public static String UrlDecode(this String url) => HttpUtility.UrlDecode(url);

    /// <summary>
    /// Url解码
    /// </summary>
    /// <param name="url">url</param>
    /// <param name="encoding">字符编码</param>
    /// <returns></returns>
    public static String UrlDecode(this String url, Encoding encoding) => HttpUtility.UrlDecode(url, encoding);

    #endregion

    #region 引用地址
    /// <summary>
    /// 引用地址
    /// </summary>
    public static String? RefererUrl => Request?.Headers.Referer;
    #endregion

    #region Url(请求地址)

    /// <summary>
    /// 获得请求的原始url(未转义)
    /// </summary>
    public static String? Url => Request?.GetDisplayUrl();

    /// <summary>
    /// 获得请求的原始url(转义)
    /// </summary>
    public static String? EncodedUrl => Request?.GetEncodedUrl();

    #endregion

    #region Host(主机)

    /// <summary>
    /// 主机
    /// </summary>
    public static String Host => Pek.Webs.HttpContext.Current == null ? Dns.GetHostName() : GetClientHostName();

    /// <summary>
    /// 获取Web客户端主机名
    /// </summary>
    /// <returns></returns>
    private static String GetClientHostName()
    {
        var address = GetRemoteAddress();
        if (String.IsNullOrWhiteSpace(address))
        {
            return Dns.GetHostName();
        }
        var result = Dns.GetHostEntry(IPAddress.Parse(address)).HostName;
        if (result == "localhost.localdomain")
        {
            result = Dns.GetHostName();
        }
        return result;
    }

    /// <summary>
    /// 获取远程地址
    /// </summary>
    /// <returns></returns>
    private static String? GetRemoteAddress()
    {
        return Pek.Webs.HttpContext.Current?.Request?.Headers["HTTP_X_FORWARDED_FOR"] ??
               Pek.Webs.HttpContext.Current?.Request?.Headers["REMOTE_ADDR"];
    }

    #endregion

    #region ContentType(内容类型)

    /// <summary>
    /// 内容类型
    /// </summary>
    public static String? ContentType => Pek.Webs.HttpContext.Current?.Request?.ContentType;

    #endregion

    #region QueryString(参数)

    /// <summary>
    /// 参数
    /// </summary>
    public static String? QueryString => Pek.Webs.HttpContext.Current?.Request?.QueryString.ToString();

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

    #region IsLocal(是否本地请求)

    /// <summary>
    /// 是否本地请求
    /// </summary>
    public static Boolean IsLocal
    {
        get
        {
            var connection = Pek.Webs.HttpContext.Current?.Request?.HttpContext?.Connection;
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            if (connection.RemoteIpAddress?.IsSet() == true)
            {
                return connection.LocalIpAddress?.IsSet() == true
                    ? connection.RemoteIpAddress.Equals(connection.LocalIpAddress)
                    : IPAddress.IsLoopback(connection.RemoteIpAddress);
            }

            return true;
        }
    }

    /// <summary>
    /// 空IP地址
    /// </summary>
    private const String NullIpAddress = "::1";

    /// <summary>
    /// 是否已设置IP地址
    /// </summary>
    /// <param name="address">IP地址</param>
    private static Boolean IsSet(this IPAddress address) => address != null && address.ToString() != NullIpAddress;

    #endregion

    /// <summary>
    /// 获取当前站点Url
    /// </summary>
    /// <returns></returns>
    public static String GetSiteUrl() => Request?.Scheme + "://" + Request?.Host;

    #region RootPath(根路径)

    /// <summary>
    /// 根路径
    /// </summary>
    public static String? RootPath => Environment?.ContentRootPath;

    #endregion

    #region WebRootPath(Web根路径)

    /// <summary>
    /// Web根路径，即wwwroot
    /// </summary>
    public static String? WebRootPath => Environment?.WebRootPath;

    #endregion

    #region Client( Web客户端 )

    /// <summary>
    /// Web客户端，用于发送Http请求
    /// </summary>
    /// <returns></returns>
    public static Pek.Webs.Clients.WebClient Client() => new();

    /// <summary>
    /// Web客户端，用于发送Http请求
    /// </summary>
    /// <typeparam name="TResult">返回结果类型</typeparam>
    /// <returns></returns>
    public static Pek.Webs.Clients.WebClient<TResult> Client<TResult>() where TResult : class => new();

    #endregion

    #region DownloadAsync(读取本地文件提供下载)

    /// <summary>
    /// 读取本地文件提供下载
    /// </summary>
    /// <param name="filePath">文件绝对路径</param>
    /// <param name="fileName">文件名。包含扩展名</param>
    public static async Task DownloadFileAsync(String filePath, String fileName) => await DownloadFileAsync(filePath, fileName, Encoding.UTF8).ConfigureAwait(false);

    /// <summary>
    /// 读取本地文件提供下载
    /// </summary>
    /// <param name="filePath">文件绝对路径</param>
    /// <param name="fileName">文件名。包含扩展名</param>
    /// <param name="encoding">字符编码</param>
    public static async Task DownloadFileAsync(String filePath, String fileName, Encoding encoding)
    {
        var bytes = FileUtil.ReadToBytes(filePath);
        await DownloadAsync(bytes, fileName, encoding).ConfigureAwait(false);
    }

    /// <summary>
    /// 读取本地文件提供下载
    /// </summary>
    /// <param name="stream">流</param>
    /// <param name="fileName">文件名。包含扩展名</param>
    public static async Task DownloadAsync(Stream stream, String fileName) => await DownloadAsync(stream, fileName, Encoding.UTF8).ConfigureAwait(false);

    /// <summary>
    /// 读取本地文件提供下载
    /// </summary>
    /// <param name="stream">流</param>
    /// <param name="fileName">文件名。包含扩展名</param>
    /// <param name="encoding">字符编码</param>
    public static async Task DownloadAsync(Stream stream, String fileName, Encoding encoding) => await DownloadAsync(await FileUtil.ToBytesAsync(stream).ConfigureAwait(false), fileName, encoding).ConfigureAwait(false);

    /// <summary>
    /// 读取本地文件提供下载
    /// </summary>
    /// <param name="bytes">字节流</param>
    /// <param name="fileName">文件名。包含扩展名</param>
    public static async Task DownloadAsync(Byte[] bytes, String fileName) => await DownloadAsync(bytes, fileName, Encoding.UTF8).ConfigureAwait(false);

    /// <summary>
    /// 读取本地文件提供下载
    /// </summary>
    /// <param name="bytes">字节流</param>
    /// <param name="fileName">文件名。包含扩展名</param>
    /// <param name="encoding">字符编码</param>
    /// <returns></returns>
    public static async Task DownloadAsync(Byte[]? bytes, String fileName, Encoding encoding)
    {
        if (bytes == null || bytes.Length == 0)
        {
            return;
        }

        if (Response == null)
        {
            return;
        }

        fileName = fileName.Replace(" ", "");
        fileName = UrlEncode(fileName, encoding);
        Response.ContentType = "application/octet-stream";

        Response.Headers[HeaderNames.ContentDisposition] = new ContentDispositionHeaderValue("attachment")
        {
            FileNameStar = fileName
        }.ToString();
        Response.Headers.ContentLength = bytes.Length;

        await Response.Body.WriteAsync(bytes).ConfigureAwait(false);
    }

    #endregion

    #region GetFiles(获取客户端文件集合)

    /// <summary>
    /// 获取客户端文件集合
    /// </summary>
    /// <returns></returns>
    public static List<IFormFile> GetFiles()
    {
        var result = new List<IFormFile>();
        var files = Pek.Webs.HttpContext.Current.Request.Form.Files;
        if (files == null || files.Count == 0)
        {
            return result;
        }

        result.AddRange(files.Where(file => file?.Length > 0));
        return result;
    }

    #endregion

    #region GetFile(获取客户端文件)

    /// <summary>
    /// 获取客户端文件
    /// </summary>
    /// <returns></returns>
    public static IFormFile? GetFile()
    {
        var files = GetFiles();
        return files.Count == 0 ? null : files[0];
    }

    #endregion

    #region GetParam(获取请求参数)

    /// <summary>
    /// 获取请求参数，搜索路径：查询参数->表单参数->请求头
    /// </summary>
    /// <param name="name">参数名</param>
    public static String? GetParam(String name)
    {
        if (String.IsNullOrWhiteSpace(name))
            return String.Empty;
        if (Request == null)
            return String.Empty;
        var result = String.Empty;
        if (Request.Query != null)
            result = Request.Query[name];
        if (String.IsNullOrWhiteSpace(result) == false)
            return result;
        if (Request.Form != null)
            result = Request.Form[name];
        if (String.IsNullOrWhiteSpace(result) == false)
            return result;
        if (Request.Headers != null)
            result = Request.Headers[name];
        return result;
    }

    #endregion

    #region Redirect(跳转到指定链接)

    /// <summary>
    /// 跳转到指定链接
    /// </summary>
    /// <param name="url">链接</param>
    public static void Redirect(String url) => Response?.Redirect(url);

    #endregion

    #region Write(输出内容)

    /// <summary>
    /// 输出内容
    /// </summary>
    /// <param name="text">内容</param>
    public static void Write(String text)
    {
        if (Response == null)
            return;

        Response.ContentType = "text/plain;charset=utf-8";
        Task.Run(async () => { await Response.WriteAsync(text).ConfigureAwait(false); }).GetAwaiter().GetResult();
    }

    #endregion

    #region Write(输出文件)

    /// <summary>
    /// 输出文件
    /// </summary>
    /// <param name="stream">文件流</param>
    public static void Write(FileStream stream)
    {
        if (Response == null) return;

        var size = stream.Length;
        var buffer = new Byte[size];
        _ = stream.Read(buffer, 0, (Int32)size);
        stream.Dispose();
        File.Delete(stream.Name);

        Response.ContentType = "application/octet-stream";
        Response.Headers[HeaderNames.ContentDisposition] = new ContentDispositionHeaderValue("attachment")
        {
            FileNameStar = WebUtility.UrlEncode(Path.GetFileName(stream.Name))
        }.ToString();
        Response.Headers.ContentLength = size;

        Task.Run(async () => { await Response.Body.WriteAsync(buffer.AsMemory(0, (Int32)size)).ConfigureAwait(false); }).GetAwaiter().GetResult();
        Response.Body.Close();
    }

    #endregion

    /// <summary>
    /// 返回绝对地址
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static String AbsoluteUri(this HttpRequest request)
    {
        var absoluteUri = String.Concat(
                      request.Scheme,
                      "://",
                      request.Host.ToUriComponent(),
                      request.PathBase.ToUriComponent(),
                      request.Path.ToUriComponent(),
                      request.QueryString.ToUriComponent());

        return absoluteUri;
    }

    public enum AgentType
    {
        Android = 0,
        IPhone = 1,
        IPad = 2,
        WindowsPhone = 3,
        Windows = 4,
        Wechat = 6,
        MacOS = 7
    }

    /// <summary>
    /// 获取客户端信息
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static AgentType UserAgentType(this HttpRequest request)
    {
        var userAgent = request.Headers.UserAgent;
        switch (userAgent)
        {
            case String android when android.Contains("MicroMessenger"):
                return AgentType.Wechat;
            case String android when android.Contains("Android"):
                return AgentType.Android;
            case String android when android.Contains("iPhone"):
                return AgentType.IPhone;
            case String android when android.Contains("iPad"):
                return AgentType.IPad;
            case String android when android.Contains("Windows Phone"):
                return AgentType.WindowsPhone;
            case String android when android.Contains("Windows NT"):
                return AgentType.Windows;
            case String android when android.Contains("Mac OS"):
                return AgentType.MacOS;
            default:
                break;
        }
        return AgentType.Android;
    }

    #region UserAgent(用户代理)

    /// <summary>
    /// 用户代理
    /// </summary>
    public static String? UserAgent => Request?.Headers.UserAgent;

    #endregion

    #region 远程下载和解压

    /// <summary>根据提供的下载地址和文件名，下载到目标目录，解压Zip后返回目标文件</summary>
    /// <param name="url">提供下载地址,不需要包含下载文件名称</param>
    /// <param name="name">页面上指定名称的链接</param>
    /// <param name="destdir">要下载到的目标目录</param>
    /// <param name="overwrite">是否覆盖目标同名文件</param>
    /// <returns></returns>
    public static async Task DownloadLinkAndExtract(String url, String name, String destdir, Boolean overwrite = false)
    {
        try
        {
            XTrace.Log.Info("[DHWeb.DownloadLinkAndExtract]:下载链接 {0}，目标 {1}", url, name);

            // 指定保存文件的目录和文件名
            var savePath = destdir.CombinePath(name); // 替换为你想保存的路径
            savePath.EnsureDirectory();

            var sw = Stopwatch.StartNew();
            var responseData = await Client().Get(UrlHelper.Combine(url, name)).DownloadDataAsync().ConfigureAwait(false);
            sw.Stop();

            if (responseData == null || responseData.Length == 0)
            {
                XTrace.WriteLine($"[DHWeb.DownloadLinkAndExtract]下载{name}失败");
                return;
            }

            var saveFile = savePath.AsFile();
            if (saveFile.Exists)
            {
                saveFile.Delete();
            }

            FileUtil.Write(savePath, responseData);                               // 保存文件

            XTrace.Log.Info("[DHWeb.DownloadLinkAndExtract]下载完成，共{0:n0}字节，耗时{1:n0}毫秒", savePath.AsFile().Length, sw.ElapsedMilliseconds);

            savePath.AsFile().Extract(destdir, overwrite);

            XTrace.Log.Info("[DHWeb.DownloadLinkAndExtract]解压缩到 {0}", destdir);
        }
        catch(Exception ex)
        {
            XTrace.WriteException(ex);
        }
    }

    #endregion
}
