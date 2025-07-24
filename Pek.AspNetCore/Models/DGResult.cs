using Microsoft.AspNetCore.Mvc;

using NewLife;

using Pek.Helpers;

namespace Pek.Models;

/// <summary>
/// 专用返回（泛型版本）
/// </summary>
/// <typeparam name="T1">Data 的类型</typeparam>
/// <typeparam name="T2">ExtData 的类型</typeparam>
public class DGResult<T1, T2> : JsonResult
{
    /// <summary>
    /// 状态码
    /// </summary>
    public StateCode Code { get; set; } = StateCode.Fail;

    /// <summary>
    /// 错误码
    /// </summary>
    public Int32 ErrCode { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    public String? Message { get; set; }

    /// <summary>
    /// 数据
    /// </summary>
    public T1? Data { get; set; }

    /// <summary>
    /// 其他数据
    /// </summary>
    public T2? ExtData { get; set; }

    /// <summary>
    /// 操作时间
    /// </summary>
    public DateTime OperationTime { get; set; }

    /// <summary>
    /// 标识
    /// </summary>
    public String? Id { get; set; }

    /// <summary>
    /// 初始化返回结果
    /// </summary>
    public DGResult() : base(null)
    {
        Code = StateCode.Fail;
        OperationTime = DateTime.Now;
    }

    /// <summary>
    /// 初始化返回结果
    /// </summary>
    /// <param name="code">状态码</param>
    /// <param name="message">消息</param>
    /// <param name="data">数据</param>
    /// <param name="extdata">其他数据</param>
    public DGResult(StateCode code, String message, T1? data = default, T2? extdata = default) : base(null)
    {
        Code = code;
        Message = message;
        Data = data;
        OperationTime = DateTime.Now;
        ExtData = extdata;
        Id = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// 执行结果
    /// </summary>
    public override Task ExecuteResultAsync(ActionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (Id.IsNullOrWhiteSpace())
            Id = Guid.NewGuid().ToString();

        Value = new
        {
            Code = Code.Value(),
            Message,
            OperationTime,
            Data,
            ExtData,
            Id,
            ErrCode,
        };
        return base.ExecuteResultAsync(context);
    }
}

/// <summary>
/// 专用返回（单泛型版本）
/// </summary>
/// <typeparam name="T">Data 的类型</typeparam>
public class DGResult<T> : DGResult<T, Object?>
{
    /// <summary>
    /// 初始化返回结果
    /// </summary>
    public DGResult() : base() { }

    /// <summary>
    /// 初始化返回结果
    /// </summary>
    /// <param name="code">状态码</param>
    /// <param name="message">消息</param>
    /// <param name="data">数据</param>
    /// <param name="extdata">其他数据</param>
    public DGResult(StateCode code, String message, T? data = default, Object? extdata = null) 
        : base(code, message, data, extdata) { }
}

/// <summary>
/// 专用返回（非泛型版本，向后兼容）
/// </summary>
public class DGResult : DGResult<Object?, Object?>
{
    /// <summary>
    /// 初始化返回结果
    /// </summary>
    public DGResult() : base() { }

    /// <summary>
    /// 初始化返回结果
    /// </summary>
    /// <param name="code">状态码</param>
    /// <param name="message">消息</param>
    /// <param name="data">数据</param>
    /// <param name="extdata">其他数据</param>
    public DGResult(StateCode code, String message, Object? data = null, Object? extdata = null) 
        : base(code, message, data, extdata) { }
}