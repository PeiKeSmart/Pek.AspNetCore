using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Pek.ResumeFileResult.Executor;

/// <summary>
/// 使用本地虚拟路径的可断点续传的FileResult
/// </summary>
internal class ResumeVirtualFileResultExecutor : VirtualFileResultExecutor, IActionResultExecutor<ResumeVirtualFileResult>
{
    /// <summary>
    /// 执行FileResult
    /// </summary>
    /// <param name="context"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public virtual Task ExecuteAsync(ActionContext context, ResumeVirtualFileResult result)
    {
        ArgumentNullException.ThrowIfNull(context);

        ArgumentNullException.ThrowIfNull(result);

        context.SetContentDispositionHeaderInline(result);

        return base.ExecuteAsync(context, result);
    }

    public ResumeVirtualFileResultExecutor(ILoggerFactory loggerFactory, IWebHostEnvironment hostingEnvironment) : base(loggerFactory, hostingEnvironment)
    {
    }
}