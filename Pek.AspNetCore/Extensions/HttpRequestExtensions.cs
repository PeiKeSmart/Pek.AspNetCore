using System.Net;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.Extensions.Primitives;

using NewLife;

namespace Pek;

/// <summary>
/// Http请求(<see cref="HttpRequest"/>) 扩展
/// </summary>
public static class HttpRequestExtensions
{
    #region GetAbsoluteUri(获取Http请求的绝对路径)

    /// <summary>
    /// 获取Http请求的绝对路径
    /// </summary>
    /// <param name="request">Http请求</param>
    public static String GetAbsoluteUri(this HttpRequest request) => new StringBuilder()
        .Append(request.Scheme)
        .Append("://")
        .Append(request.Host)
        .Append(request.PathBase)
        .Append(request.Path)
        .Append(request.QueryString)
        .ToString();

    #endregion

    #region Query(获取查询参数)

    /// <summary>
    /// 获取查询参数
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="request">Http请求</param>
    /// <param name="key">键</param>
    /// <param name="defaultValue">默认值</param>
    public static T? Query<T>(this HttpRequest request, String key, T? defaultValue = default)
        where T : IConvertible
    {
        var value = request.Query.FirstOrDefault(x => x.Key == key);
        if (String.IsNullOrWhiteSpace(value.Value.ToString()))
            return defaultValue;
        try
        {
            return (T)Convert.ChangeType(value.Value.ToString(), typeof(T));
        }
        catch (InvalidCastException)
        {
            return defaultValue;
        }
    }

    #endregion

    #region Form(获取表单参数)

    /// <summary>
    /// 获取表单参数
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="request">Http请求</param>
    /// <param name="key">键</param>
    /// <param name="defaultValue">默认值</param>
    public static T? Form<T>(this HttpRequest request, String key, T? defaultValue = default)
        where T : IConvertible
    {
        var value = request.Form.FirstOrDefault(x => x.Key == key);
        if (String.IsNullOrWhiteSpace(value.Value.ToString()))
            return defaultValue;
        try
        {
            return (T)Convert.ChangeType(value.Value.ToString(), typeof(T));
        }
        catch (InvalidCastException)
        {
            return defaultValue;
        }
    }

    #endregion

    #region Params(获取参数)

    /// <summary>
    /// 获取参数
    /// </summary>
    /// <param name="request">请求信息</param>
    /// <param name="key">键名</param>
    public static String? Params(this HttpRequest request, String key)
    {
        if (request.Query.ContainsKey(key))
            return request.Query[key];
        if (request.HasFormContentType)
            return request.Form[key];
        return null;
    }

    #endregion

    #region IsJsonContentType(是否Json内容类型)

    /// <summary>
    /// 是否Json内容类型
    public static Boolean IsJsonContentType(this HttpRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var flag =
            request.Headers?["Content-Type"].ToString()
                .IndexOf("application/json", StringComparison.OrdinalIgnoreCase) > -1 || request
                .Headers?["Content-Type"].ToString().IndexOf("text/json", StringComparison.OrdinalIgnoreCase) > -1;

        if (flag)
            return true;
        flag =
            request.Headers?["Accept"].ToString().IndexOf("application/json", StringComparison.OrdinalIgnoreCase) >
            -1 || request.Headers?["Accept"].ToString().IndexOf("text/json", StringComparison.OrdinalIgnoreCase) >
            -1;
        return flag;
    }

    #endregion

    #region IsMobileBrowser(是否移动端浏览器)

    /// <summary>
    /// 浏览器正则表达式
    /// </summary>
    private static readonly Regex BrowserRegex = new(
        @"(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino",
        RegexOptions.IgnoreCase | RegexOptions.Multiline);

    /// <summary>
    /// 版本号正则表达式
    /// </summary>
    private static readonly Regex VersionRegex = new(
        @"1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-",
        RegexOptions.IgnoreCase | RegexOptions.Multiline);

    /// <summary>
    /// 是否移动端浏览器
    /// </summary>
    /// <param name="request">Http请求</param>
    public static Boolean IsMobileBrowser(this HttpRequest request)
    {
        var userAgent = request.UserAgent();
        if (userAgent.IsNullOrWhiteSpace())
        {
            return false;
        }
        return BrowserRegex.IsMatch(userAgent) || VersionRegex.IsMatch(userAgent.Substring(0, 4));
    }

    #endregion

    #region UserAgent(用户代理)

    /// <summary>
    /// 用户代理
    /// </summary>
    /// <param name="request">Http请求</param>
    public static String? UserAgent(this HttpRequest request) => request.Headers["User-Agent"];

    #endregion

    /// <summary>
    /// 检查请求是否为 POST 请求
    /// </summary>
    /// <param name="request">请求检查</param>
    /// <returns>如果请求是 POST 请求，则返回 true，否则在所有其他情况下返回 false</returns>
    public static Boolean IsPostRequest(this HttpRequest request) => request.Method.Equals(WebRequestMethods.Http.Post, StringComparison.InvariantCultureIgnoreCase);

    /// <summary>
    /// 检查请求是否为 GET 请求
    /// </summary>
    /// <param name="request">要检查的请求</param>
    /// <returns>如果请求是 GET 请求，则返回 true，所有其他情况返回 false</returns>
    public static Boolean IsGetRequest(this HttpRequest request) => request.Method.Equals(WebRequestMethods.Http.Get, StringComparison.InvariantCultureIgnoreCase);

    /// <summary>
    /// 获取表单值
    /// </summary>
    /// <param name="request">请求</param>
    /// <param name="formKey">表单密钥</param>
    /// <returns>
    /// 一个表示异步操作的任务
    /// 任务结果包含表单值
    /// </returns>
    public static async Task<StringValues> GetFormValueAsync(this HttpRequest request, String formKey)
    {
        if (!request.HasFormContentType)
            return new StringValues();

        var form = await request.ReadFormAsync();

        return form[formKey];
    }

    /// <summary>
    /// 检查提供的键是否在表单中存在
    /// </summary>
    /// <param name="request">请求</param>
    /// <param name="formKey">表单键</param>
    /// <returns>
    /// 一个表示异步操作的任务
    /// 任务结果包含 true，如果键以表单形式持续存在，否则为 false
    /// </returns>
    public static async Task<Boolean> IsFormKeyExistsAsync(this HttpRequest request, String formKey) => await IsFormAnyAsync(request, key => key.Equals(formKey));

    /// <summary>
    /// 检查键是否在表单中存在
    /// </summary>
    /// <param name="request">请求</param>
    /// <param name="predicate">筛选条件。设置为 null 表示无需筛选</param>
    /// <returns>
    /// 一个表示异步操作的任务
    /// 任务结果包含 true，如果任何项目以表单形式持续存在，否则为 false
    /// </returns>
    public static async Task<Boolean> IsFormAnyAsync(this HttpRequest request, Func<String, Boolean>? predicate = null)
    {
        if (!request.HasFormContentType)
            return false;

        var form = await request.ReadFormAsync();

        return predicate == null ? form.Any() : form.Keys.Any(predicate);
    }

    /// <summary>
    /// 获取指定表单键对应的值
    /// </summary>
    /// <param name="request">请求</param>
    /// <param name="formKey">表单密钥</param>
    /// <returns>
    /// 一个表示异步操作的任务
    /// 任务结果包含 true 和表单值，如果表单包含具有指定键的元素；否则，false 和默认值。
    /// </returns>
    public static async Task<(bool keyExists, StringValues formValue)> TryGetFormValueAsync(this HttpRequest request, String formKey)
    {
        if (!request.HasFormContentType)
            return (false, default);

        var form = await request.ReadFormAsync();

        var flag = form.TryGetValue(formKey, out var formValue);

        return (flag, formValue);
    }

    /// <summary>
    /// 返回 Form.Files 的首个元素，如果序列中无元素则返回默认值
    /// </summary>
    /// <param name="request">请求</param>
    /// <returns>
    /// 一个表示异步操作的任务
    /// 任务结果包含 <see cref="IFormFile"/> 元素或默认值
    /// </returns>
    public static async Task<IFormFile?> GetFirstOrDefaultFileAsync(this HttpRequest request)
    {
        if (!request.HasFormContentType)
            return default;

        var form = await request.ReadFormAsync();

        return form.Files.FirstOrDefault();
    }
}