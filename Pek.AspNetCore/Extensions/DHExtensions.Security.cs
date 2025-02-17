﻿using System.Security.Claims;

using Pek.Security.Principals;

namespace Pek;

/// <summary>
/// 系统扩展 - 安全
/// </summary>
public static partial class Extensions
{
    /// <summary>
    /// 获取用户标识声明值
    /// </summary>
    /// <param name="identity">用户标识</param>
    /// <param name="type">声明类型</param>
    public static String GetValue(this ClaimsIdentity identity, String type)
    {
        var claim = identity.FindFirst(type);
        if (claim == null)
            return String.Empty;
        return claim.Value;
    }

    /// <summary>
    /// 获取身份标识
    /// </summary>
    /// <param name="context">Http上下文</param>
    public static ClaimsIdentity GetIdentity(this HttpContext context)
    {
        if (context == null)
            return UnauthenticatedIdentity.Instance;
        if (context.User is not ClaimsPrincipal principal)
            return UnauthenticatedIdentity.Instance;
        if (principal.Identity is ClaimsIdentity identity)
            return identity;
        return UnauthenticatedIdentity.Instance;
    }
}