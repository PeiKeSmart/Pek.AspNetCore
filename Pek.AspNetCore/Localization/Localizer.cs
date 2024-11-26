namespace Pek.Localization;

/// <summary>
/// 定位器
/// </summary>
/// <param name="text">文本内容</param>
/// <param name="args">文本内容的参数</param>
/// <returns>本地化字符串</returns>
public delegate LocalizedString Localizer(String? text, params Object[] args);