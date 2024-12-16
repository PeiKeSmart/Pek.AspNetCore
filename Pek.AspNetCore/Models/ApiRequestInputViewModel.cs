namespace Pek.Models;

public class ApiRequestInputViewModel
{
    /// <summary>
    /// 请求接口名称（中文）
    /// </summary>
    public String RequestName { get; set; }

    /// <summary>
    /// 请求来源IP
    /// </summary>
    public String? RequestIP { get; set; }

    /// <summary>
    /// 请求路径
    /// </summary>
    public String RequestUrl { get; set; }

    /// <summary>
    /// 请求类型：GET/POST
    /// </summary>
    public String HttpType { get; set; }

    /// <summary>
    /// 请求参数字符串
    /// </summary>
    public String? Query { get; set; }

    /// <summary>
    /// 请求报文，POST专用
    /// </summary>
    public String Body { get; set; }

    public String RequestTime { get; set; }

    public String ResponseBody { get; set; }

    public Int64 ElapsedTime { get; set; }

    /// <summary>
    /// 请求的头部数据
    /// </summary>
    public String RequestHeader { get; set; }

    public ApiRequestInputViewModel()
    {
        RequestName = String.Empty;
        RequestIP = String.Empty;
        RequestUrl = String.Empty;
        HttpType = String.Empty;
        Query = String.Empty;
        RequestTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        Body = String.Empty;
        ResponseBody = String.Empty;
        ElapsedTime = -1;
        RequestHeader = String.Empty;
    }
}
