using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Pek.ResumeFileResult.Executor;

/// <summary>
/// 可断点续传的FileStreamResult执行器
/// </summary>
internal class ResumeFileStreamResultExecutor : FileStreamResultExecutor, IActionResultExecutor<ResumeFileStreamResult>
{
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="loggerFactory"></param>
    public ResumeFileStreamResultExecutor(ILoggerFactory loggerFactory) : base(loggerFactory)
    {
    }

    /// <summary>
    /// 执行Result
    /// </summary>
    /// <param name="context"></param>
    /// <param name="result"></param>
    /// <returns></returns>
    public virtual Task ExecuteAsync(ActionContext context, ResumeFileStreamResult result)
    {
        ArgumentNullException.ThrowIfNull(context);

        ArgumentNullException.ThrowIfNull(result);

        context.SetContentDispositionHeaderInline(result);

        return base.ExecuteAsync(context, result);
    }
}