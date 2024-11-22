namespace Pek.Webs;

/// <summary>
/// 表示一个网页助手
/// </summary>
public partial interface IWebHelper
{
    /// <summary>
    /// 如果存在，获取 URL 来源
    /// </summary>
    /// <returns>URL 访问者</returns>
    String? GetUrlReferrer();

    /// <summary>
    /// 从 HTTP 上下文中获取 IP 地址
    /// </summary>
    /// <returns>IP 地址字符串</returns>
    String GetCurrentIpAddress();

    /// <summary>
    /// 获取当前页面的 URL
    /// </summary>
    /// <param name="includeQueryString">表示是否包括查询字符串的值</param>
    /// <param name="useSsl">表示是否获取 SSL 安全页面 URL 的值。传递 null 以自动确定</param>
    /// <param name="lowercaseUrl">表示是否将 URL 转换为小写</param>
    /// <returns>页面 URL</returns>
    String GetThisPageUrl(Boolean includeQueryString, Boolean? useSsl = null, Boolean lowercaseUrl = false);

    /// <summary>
    /// 获取一个值，表示当前连接是否安全
    /// </summary>
    /// <returns>如果它是安全的，则返回真，否则返回假</returns>
    Boolean IsCurrentConnectionSecured();

    /// <summary>
    /// 获取商店主机位置
    /// </summary>
    /// <param name="useSsl">是否获取 SSL 安全的 URL</param>
    /// <returns>存储主机位置</returns>
    String GetStoreHost(Boolean useSsl);

    /// <summary>
    /// 获取商店位置
    /// </summary>
    /// <param name="useSsl">是否获取 SSL 安全的 URL；传递 null 以自动决定</param>
    /// <returns>存储位置</returns>
    String GetStoreLocation(Boolean? useSsl = null);

    /// <summary>
    /// 如果请求的资源是不需要 CMS 引擎处理的典型资源之一，则返回 true。
    /// </summary>
    /// <returns>如果请求针对的是静态资源文件，则返回真。</returns>
    Boolean IsStaticResource();

    /// <summary>
    /// 修改 URL 的查询字符串
    /// </summary>
    /// <param name="url">要修改的网址</param>
    /// <param name="key">要添加的查询参数键</param>
    /// <param name="values">要添加的查询参数值</param>
    /// <returns>包含传递查询参数的新 URL</returns>
    String ModifyQueryString(String url, String key, params String[] values);

    /// <summary>
    /// 从 URL 中移除查询参数
    /// </summary>
    /// <param name="url">要修改的 URL</param>
    /// <param name="key">要删除的查询参数键</param>
    /// <param name="value">要删除的查询参数值；传递 null 以删除具有指定键的所有查询参数</param>
    /// <returns>不包含传递查询参数的新 URL</returns>
    String RemoveQueryString(String url, String key, String? value = null);

    /// <summary>
    /// 通过名称获取查询字符串值
    /// </summary>
    /// <typeparam name="T">返回值类型</typeparam>
    /// <param name="name">查询参数名称</param>
    /// <returns>查询字符串值</returns>
    T? QueryString<T>(String name);

    /// <summary>
    /// 重启应用程序域
    /// </summary>
    void RestartAppDomain();

    /// <summary>
    /// 获取一个值，表示客户端是否被重定向到新的位置
    /// </summary>
    Boolean IsRequestBeingRedirected { get; }

    /// <summary>
    /// 获取或设置一个值，指示客户端是否正在使用 POST 重定向到新位置
    /// </summary>
    Boolean IsPostBeingDone { get; set; }

    /// <summary>
    /// 获取当前 HTTP 请求协议
    /// </summary>
    String GetCurrentRequestProtocol();

    /// <summary>
    /// 判断指定的 HTTP 请求 URI 是否引用了本地主机。
    /// </summary>
    /// <param name="req">HTTP 请求</param>
    /// <returns>如果 HTTP 请求 URI 引用到本地主机，则返回 True</returns>
    Boolean IsLocalRequest(HttpRequest req);

    /// <summary>
    /// 获取请求的原始路径和完整查询
    /// </summary>
    /// <param name="request">HTTP 请求</param>
    /// <returns>原始 URL</returns>
    String GetRawUrl(HttpRequest request);

    /// <summary>
    /// 判断请求是否使用了 AJAX 
    /// </summary>
    /// <param name="request">HTTP 请求</param>
    /// <returns>结果</returns>
    Boolean IsAjaxRequest(HttpRequest request);
}