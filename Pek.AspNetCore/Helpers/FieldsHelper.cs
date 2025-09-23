using System.Collections.Concurrent;
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

    /// <summary>记录区域是否为后台：areaName -> isAdmin</summary>
    public static readonly ConcurrentDictionary<String, Boolean> AdminFlags = new(StringComparer.OrdinalIgnoreCase);
}