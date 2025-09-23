using System.Reflection;

using NewLife.Model;

using Pek.Infrastructure;

namespace Pek;

/// <summary>
/// 字段帮助类
/// </summary>
public class FieldsHelper
{
    /// <summary>区域名集合</summary>
    public static String[]? AreaNames { get; set; }

    /// <summary>文件Assembly集合</summary>
    public static IList<Assembly> Assemblies { get; set; } = ObjectContainer.Provider.GetPekService<ITypeFinder>()?.GetAssemblies() ?? [];
}