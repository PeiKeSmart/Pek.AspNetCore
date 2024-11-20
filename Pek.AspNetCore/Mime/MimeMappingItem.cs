namespace Pek.Mime;

/// <summary>
/// 表示一个 MIME 映射项，用于将文件扩展名映射到 MIME 类型。
/// </summary>
public class MimeMappingItem
{
    /// <summary>
    /// 扩展名
    /// </summary>
    public String? Extension { get; set; }

    /// <summary>
    /// 文件类型
    /// </summary>
    public String? MimeType { get; set; }
}