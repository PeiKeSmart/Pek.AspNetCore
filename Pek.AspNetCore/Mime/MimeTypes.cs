namespace Pek.Mime;

/// <summary>
/// MIME 类型常量的集合，用于避免拼写错误
/// 如果需要，可以自由添加缺失的 MimeTypes
/// </summary>
public static class MimeTypes
{
    #region application/*

    /// <summary>
    /// 类型
    /// </summary>
    public static String ApplicationForceDownload => "application/force-download";

    /// <summary>
    /// 类型
    /// </summary>
    public static String ApplicationJson => "application/json";

    /// <summary>
    /// 类型
    /// </summary>
    public static String ApplicationManifestJson => "application/manifest+json";

    /// <summary>
    /// 类型
    /// </summary>
    public static String ApplicationOctetStream => "application/octet-stream";

    /// <summary>
    /// 类型
    /// </summary>
    public static String ApplicationPdf => "application/pdf";

    /// <summary>
    /// 类型
    /// </summary>
    public static String ApplicationRssXml => "application/rss+xml";

    /// <summary>
    /// 类型
    /// </summary>
    public static String ApplicationXml => "application/xml";

    /// <summary>
    /// 类型
    /// </summary>
    public static String ApplicationXWwwFormUrlencoded => "application/x-www-form-urlencoded";

    /// <summary>
    /// 类型
    /// </summary>
    public static String ApplicationZip => "application/zip";

    /// <summary>
    /// 类型
    /// </summary>
    public static String ApplicationXZipCo => "application/x-zip-co";

    #endregion

    #region image/*

    /// <summary>
    /// 类型
    /// </summary>
    public static String ImageBmp => "image/bmp";

    /// <summary>
    /// 类型
    /// </summary>
    public static String ImageGif => "image/gif";

    /// <summary>
    /// 类型
    /// </summary>
    public static String ImageJpeg => "image/jpeg";

    /// <summary>
    /// 类型
    /// </summary>
    public static String ImagePJpeg => "image/pjpeg";

    /// <summary>
    /// 类型
    /// </summary>
    public static String ImagePng => "image/png";

    /// <summary>
    /// 类型
    /// </summary>
    public static String ImageTiff => "image/tiff";

    /// <summary>
    /// 类型
    /// </summary>
    public static String ImageWebp => "image/webp";

    /// <summary>
    /// 类型
    /// </summary>
    public static String ImageSvg => "image/svg+xml";

    #endregion

    #region text/*

    /// <summary>
    /// 类型
    /// </summary>
    public static String TextCss => "text/css";

    /// <summary>
    /// 类型
    /// </summary>
    public static String TextCsv => "text/csv";

    /// <summary>
    /// 类型
    /// </summary>
    public static String TextJavascript => "text/javascript";

    /// <summary>
    /// 类型
    /// </summary>
    public static String TextPlain => "text/plain";

    /// <summary>
    /// 类型
    /// </summary>
    public static String TextXlsx => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    #endregion
}