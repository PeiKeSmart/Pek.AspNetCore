using Microsoft.AspNetCore.Html;

namespace Pek.Localization;

/// <summary>
/// 本地化字符串
/// </summary>
/// <remarks>
/// 实例化
/// </remarks>
/// <param name="localized">本地化内容</param>
public class LocalizedString(String? localized) : HtmlString(localized)
{
    /// <summary>
    /// 文本内容
    /// </summary>
    public String? Text { get; } = localized;
}