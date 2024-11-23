namespace Pek.Models;

public class DResult
{
    public Boolean success { get; set; }

    public String? msg { get; set; }

    /// <summary>
    /// 状态
    /// <para>1表成功</para>
    /// </summary>
    public Int32 status { get; set; }

    /// <summary>
    /// 数据
    /// </summary>
    public Object? data { get; set; }

    /// <summary>
    /// 附加数据
    /// </summary>
    public Object? extdata { get; set; }

    public Int32 code { get; set; }

    /// <summary>
    /// 网址路径
    /// </summary>
    public String? locate { get; set; }
}

public class DResult<T> : DResult
{
    public T? TData { get; set; }
}