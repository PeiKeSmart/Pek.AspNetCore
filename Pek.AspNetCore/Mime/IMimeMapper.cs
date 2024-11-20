namespace Pek.Mime;

/// <summary>
/// Mime
/// </summary>
public interface IMimeMapper
{
    /// <summary>
    /// 扩展Mime
    /// </summary>
    /// <param name="extensions"></param>
    /// <returns></returns>
    IMimeMapper Extend(params MimeMappingItem[] extensions);

    /// <summary>
    /// 根据扩展名获取mime type
    /// </summary>
    /// <param name="fileExtension"></param>
    /// <returns></returns>
    String? GetMimeFromExtension(String fileExtension);

    /// <summary>
    /// 根据路径获取Mime Type
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    String? GetMimeFromPath(String filePath);
}