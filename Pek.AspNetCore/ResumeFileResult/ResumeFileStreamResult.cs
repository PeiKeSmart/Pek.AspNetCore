using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Net.Http.Headers;

namespace Pek.ResumeFileResult;

/// <summary>
/// 基于Stream的ResumeFileStreamResult
/// </summary>
public class ResumeFileStreamResult : FileStreamResult, IResumeFileResult
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="fileStream">文件流</param>
    /// <param name="contentType">Content-Type</param>
    /// <param name="etag">ETag</param>
    public ResumeFileStreamResult(FileStream fileStream, String? contentType, String? etag = null) : this(fileStream, MediaTypeHeaderValue.Parse(contentType), !String.IsNullOrEmpty(etag) ? EntityTagHeaderValue.Parse(etag) : null)
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="fileStream">文件流</param>
    /// <param name="contentType">Content-Type</param>
    /// <param name="etag">ETag</param>
    public ResumeFileStreamResult(FileStream fileStream, MediaTypeHeaderValue contentType, EntityTagHeaderValue? etag = null) : base(fileStream, contentType)
    {
        EntityTag = etag;
        EnableRangeProcessing = true;
    }

    /// <inheritdoc/>
    public String FileInlineName { get; set; } = String.Empty;

    /// <inheritdoc/>
    public override Task ExecuteResultAsync(ActionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var executor = context.HttpContext.RequestServices.GetRequiredService<IActionResultExecutor<ResumeFileStreamResult>>();
        return executor.ExecuteAsync(context, this);
    }
}