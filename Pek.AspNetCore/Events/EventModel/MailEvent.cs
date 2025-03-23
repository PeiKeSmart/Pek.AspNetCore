using Pek.Models;

namespace Pek.Events.EventModel;

/// <summary>
/// 邮箱事件
/// </summary>
public class MailEvent
{
    public MailEvent(DResult result, Boolean checkCode, String? imgCheckCode, String account, String? lng, String sId, String sType)
    {
        Result = result;
        CheckCode = checkCode;
        ImgCheckCode = imgCheckCode;
        Account = account;
        Lng = lng;
        SId = sId;
        SType = sType;
    }

    /// <summary>
    /// 结果
    /// </summary>
    public DResult Result { get; set; }

    /// <summary>
    /// 是否检查验证码
    /// </summary>
    public Boolean CheckCode { get; set; }

    /// <summary>
    /// 图片验证码
    /// </summary>
    public String? ImgCheckCode { get; set; }

    /// <summary>
    /// 账号
    /// </summary>
    public String Account { get; set; }

    /// <summary>
    /// 语言
    /// </summary>
    public String? Lng { get; set; }

    /// <summary>
    /// 会话ID
    /// </summary>
    public String SId { get; set; }

    /// <summary>
    /// 类型
    /// </summary>
    public String SType { get; set; }
}
