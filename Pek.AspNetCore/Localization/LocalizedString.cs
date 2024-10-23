using Microsoft.AspNetCore.Html;

namespace Pek.Localization;

/// <summary>
/// 本地化字符串
/// </summary>
public class LocalizedString : HtmlString
{
    /// <summary>
    /// 文本内容
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// 实例化
    /// </summary>
    /// <param name="localized">本地化内容</param>
    public LocalizedString(string localized) : base(localized)
    {
        Text = localized;
    }
}