using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Pek.ResumeFileResult.Executor;

/// <summary>
/// 断点续传文件FileResult执行器
/// </summary>
internal class ResumeFileContentResultExecutor : FileContentResultExecutor, IActionResultExecutor<ResumeFileContentResult>
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="loggerFactory"></param>
    public ResumeFileContentResultExecutor(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
    }

    /// <summary>
    /// 执行Result
    /// </summary>
    /// <param name="context"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public virtual Task ExecuteAsync(ActionContext context, ResumeFileContentResult result)
    {
        ArgumentNullException.ThrowIfNull(context);

        ArgumentNullException.ThrowIfNull(result);

        context.SetContentDispositionHeaderInline(result);
        return base.ExecuteAsync(context, result);
    }
}