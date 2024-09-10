using Pek.Infrastructure;

namespace Pek;

/// <summary>
/// 表示公共帮助程序
/// </summary>
public partial class CommonHelper
{
    #region 属性

    /// <summary>
    /// 获取或设置默认文件提供程序
    /// </summary>
    public static IDHFileProvider DefaultFileProvider { get; set; }

    #endregion
}