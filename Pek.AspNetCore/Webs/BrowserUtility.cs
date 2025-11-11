namespace Pek.Webs;

/// <summary>
/// 浏览器公共类
/// </summary>
public static class BrowserUtility
{
    /// <summary>
    /// 获取 Headers 中的 User-Agent 字符串
    /// </summary>
    /// <param name="httpRequest"></param>
    /// <returns></returns>
    public static String? GetUserAgent(HttpRequest? httpRequest)
    {
        // 优先使用 DHWeb.UserAgent (带缓存)，如果为空则回退到直接获取
        return Helpers.DHWeb.UserAgent ?? httpRequest?.Headers.UserAgent;
    }

    /// <summary>
    /// 判断是否在微信内置浏览器中
    /// </summary>
    /// <param name="httpContext">HttpContextBase对象</param>
    /// <returns>true：在微信内置浏览器内。false：不在微信内置浏览器内。</returns>
    public static Boolean SideInWeixinBrowser(this Microsoft.AspNetCore.Http.HttpContext httpContext)
    {
        httpContext ??= Helpers.DHWeb.HttpContext;
        var userAgent = GetUserAgent(httpContext.Request)?.ToUpper();
        //判断是否在微信浏览器内部
        var isInWeixinBrowser = userAgent != null &&
                    (!userAgent.Contains(" WXWORK/", StringComparison.OrdinalIgnoreCase) && (userAgent.Contains("MICROMESSENGER", StringComparison.OrdinalIgnoreCase) || userAgent.Contains("WINDOWS PHONE", StringComparison.OrdinalIgnoreCase)/*Windows Phone*/));
        return isInWeixinBrowser;
    }

    /// <summary>
    /// 判断是否在微信小程序内发起请求（注意：此方法在Android下有效，在iOS下暂时无效！）
    /// </summary>
    /// <param name="httpContext">HttpContextBase对象</param>
    /// <returns>true：在微信内置浏览器内。false：不在微信内置浏览器内。</returns>
    public static Boolean SideInWeixinMiniProgram(this Microsoft.AspNetCore.Http.HttpContext httpContext)
    {
        var userAgent = GetUserAgent(httpContext.Request)?.ToUpper();
        //判断是否在微信小程序的 web-view 组件内部
        var isInWeixinMiniProgram = userAgent != null && userAgent.Contains("MINIPROGRAM")/*miniProgram*/;
        return isInWeixinMiniProgram;
    }

    /// <summary>
    /// 判断是否在钉钉内发起请求
    /// </summary>
    /// <param name="httpContext">HttpContextBase对象</param>
    /// <returns>true：在钉钉内置浏览器内。false：不在钉钉内置浏览器内。</returns>
    public static Boolean SideInDingTalkBrowser(this Microsoft.AspNetCore.Http.HttpContext httpContext)
    {
        var userAgent = GetUserAgent(httpContext.Request)?.ToUpper();
        //判断是否在微信小程序的 web-view 组件内部
        var isInDingTalkBrowser = userAgent != null && userAgent.Contains("DingTalk")/*dingTalk*/;
        return isInDingTalkBrowser;
    }
}