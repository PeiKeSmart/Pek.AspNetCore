namespace Pek.ResumeFileResult;

/// <summary>
/// 可断点续传的FileResult
/// </summary>
public interface IResumeFileResult
{
    /// <summary>
    /// 文件下载名
    /// </summary>
    String FileDownloadName { get; set; }

    /// <summary>
    /// 给响应头的文件名
    /// </summary>
    String FileInlineName { get; set; }
}