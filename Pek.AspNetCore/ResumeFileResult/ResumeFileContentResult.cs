using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Net.Http.Headers;

namespace Pek.ResumeFileResult;

/// <summary>
/// 基于Stream的ResumeFileContentResult
/// </summary>
public class ResumeFileContentResult : FileContentResult, IResumeFileResult
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="fileContents">文件二进制流</param>
    /// <param name="contentType">Content-Type</param>
    /// <param name="etag">ETag</param>
    public ResumeFileContentResult(Byte[] fileContents, String? contentType, String? etag = null) : this(fileContents, MediaTypeHeaderValue.Parse(contentType), !String.IsNullOrEmpty(etag) ? EntityTagHeaderValue.Parse(etag) : null)
    {
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="fileContents">文件二进制流</param>
    /// <param name="contentType">Content-Type</param>
    /// <param name="etag">ETag</param>
    public ResumeFileContentResult(Byte[] fileContents, MediaTypeHeaderValue contentType, EntityTagHeaderValue? etag = null) : base(fileContents, contentType)
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

        var executor = context.HttpContext.RequestServices.GetRequiredService<IActionResultExecutor<ResumeFileContentResult>>();
        return executor.ExecuteAsync(context, this);
    }
}