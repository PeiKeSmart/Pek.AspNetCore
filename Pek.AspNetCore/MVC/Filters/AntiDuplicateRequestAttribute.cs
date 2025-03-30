using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using NewLife.Model;

using Pek.Infrastructure;
using Pek.Locks;
using Pek.Models;
using Pek.Webs;

namespace Pek.MVC.Filters;

/// <summary>
/// 防止重复提交过滤器
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class AntiDuplicateRequestAttribute : ActionFilterAttribute
{
    /// <summary>
    /// 业务标识
    /// </summary>
    public String? Key { get; set; }

    /// <summary>
    /// 锁类型
    /// </summary>
    public LockType Type { get; set; } = LockType.User;

    /// <summary>
    /// 再次提交时间间隔，单位：秒
    /// </summary>
    public Int32 Interval { get; set; }

    /// <summary>
    /// 执行完是否自动解除锁定
    /// </summary>
    public Boolean AutoUnLock { get; set; }

    /// <summary>
    /// 执行
    /// </summary>
    /// <param name="context">操作执行上下文</param>
    /// <param name="next">操作执行下一步委托</param>
    /// <exception cref="ArgumentNullException"></exception>
    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        var @lock = CreateLock();
        var key = GetKey(context);
        try
        {
            var isSuccess = @lock.Lock(key, GetExpiration());
            if (isSuccess == false)
            {
                var result = new DResult
                {
                    msg = GetFailMessage(),
                    code = 2
                };

                context.Result = new JsonResult(result);
                return;
            }

            OnActionExecuting(context);
            if (context.Result != null)
            {
                return;
            }

            var executedContext = await next().ConfigureAwait(false);
            OnActionExecuted(executedContext);
        }
        finally
        {
            @lock.UnLock(AutoUnLock);
        }
    }

    /// <summary>
    /// 创建业务锁
    /// </summary>
    private static ILock CreateLock() => ObjectContainer.Provider.GetPekService<ILock>() ?? NullLock.Instance;

    /// <summary>
    /// 获取锁定标识
    /// </summary>
    /// <param name="context">操作执行上下文</param>
    protected virtual String GetKey(ActionExecutingContext context)
    {
        var userId = String.Empty;
        if (Type == LockType.User)
        {
            var UserId = DHWebHelper.FillDeviceId(context.HttpContext);

            userId = $"{UserId}_";
        }
        return String.IsNullOrWhiteSpace(Key) ? $"{userId}{Pek.Helpers.DHWeb.Request?.Path}" : $"{userId}{Key}";
    }

    /// <summary>
    /// 获取到期时间间隔
    /// </summary>
    private Int32 GetExpiration()
    {
        if (Interval == 0)
            return 10;
        return Interval;
    }

    /// <summary>
    /// 获取失败消息
    /// </summary>
    protected virtual String? GetFailMessage()
    {
        var _language = ObjectContainer.Provider.GetPekService<IPekLanguage>();

        if (Type == LockType.User)
            return _language?.Translate("请不要重复提交");
        return _language?.Translate("其他用户正在执行该操作,请稍后再试");
    }
}

/// <summary>
/// 锁类型
/// </summary>
public enum LockType
{
    /// <summary>
    /// 用户锁，当用户发出多个执行该操作的请求，只有第一个请求被执行，其它请求被抛弃，其它用户不受影响
    /// </summary>
    User = 0,

    /// <summary>
    /// 全局锁，该操作同时只有一个用户请求被执行
    /// </summary>
    Global = 1
}