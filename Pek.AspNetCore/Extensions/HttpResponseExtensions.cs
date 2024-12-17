using System.Text;

using NewLife.Serialization;

namespace Pek;

/// <summary>
/// Http响应(<see cref="HttpResponse"/>) 扩展
/// </summary>
public static class HttpResponseExtensions
{
    #region WriteJsonAsync(写入Json)

    /// <summary>
    /// 写入Json
    /// </summary>
    /// <param name="response">Http响应</param>
    /// <param name="obj">对象</param>
    public static async Task WriteJsonAsync(this HttpResponse response, Object obj)
    {
        var json = JsonHelper.ToJson(obj);
        await response.WriteJsonAsync(json).ConfigureAwait(false);
    }

    /// <summary>
    /// 写入Json
    /// </summary>
    /// <param name="response">Http响应</param>
    /// <param name="json">Json字符串</param>
    public static async Task WriteJsonAsync(this HttpResponse response, String json)
    {
        response.ContentType = "application/json; charset=utf-8";
        await response.WriteAsync(json).ConfigureAwait(false);
    }

    #endregion

    #region SetCache(设置缓存头)

    /// <summary>
    /// 设置缓存头
    /// </summary>
    /// <param name="response">Http响应</param>
    /// <param name="maxAge">最大有效期</param>
    public static void SetCache(this HttpResponse response, Int32 maxAge)
    {
        if (maxAge == 0)
        {
            SetNoCache(response);
        }
        else if (maxAge > 0)
        {
            if (!response.Headers.ContainsKey("Cache-Control"))
            {
                response.Headers.Append("Cache-Control", $"max-age={maxAge}");
            }
        }
    }

    #endregion

    #region SetNoCache(设置无缓存)

    /// <summary>
    /// 设置无缓存
    /// </summary>
    /// <param name="response">Http响应</param>
    public static void SetNoCache(this HttpResponse response)
    {
        if (!response.Headers.ContainsKey("Cache-Control"))
            response.Headers.Append("Cache-Control", "no-store, no-cache, max-age=0");
        if (!response.Headers.ContainsKey("Pragma"))
            response.Headers.Append("Pragma", "no-cache");
    }

    #endregion

    #region WriteHtmlAsync(写入Html)

    /// <summary>
    /// 写入Html
    /// </summary>
    /// <param name="response">Http响应</param>
    /// <param name="html">Html字符串</param>
    public static async Task WriteHtmlAsync(this HttpResponse response, String html)
    {
        response.ContentType = "text/html; charset=utf-8";
        await response.WriteAsync(html, Encoding.UTF8).ConfigureAwait(false);
    }

    #endregion

}