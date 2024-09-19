using System.Security.Claims;

using Pek.Security.Principals;

namespace Pek.Helpers;

/// <summary>
/// Web操作
/// </summary>
public static partial class DHWeb
{
    #region HttpContext(当前Http上下文)

    /// <summary>
    /// 当前Http上下文
    /// </summary>
    public static HttpContext HttpContext => Pek.Webs.HttpContext.Current;

    #endregion

    #region User(当前用户安全主体)

    /// <summary>
    /// 当前用户安全主体
    /// </summary>
    public static ClaimsPrincipal User
    {
        get
        {
            if (HttpContext == null)
                return UnauthenticatedPrincipal.Instance;
            if (HttpContext.User is ClaimsPrincipal principal)
                return principal;
            return UnauthenticatedPrincipal.Instance;
        }
    }

    #endregion

    #region Identity(当前用户身份)

    /// <summary>
    /// 当前用户身份
    /// </summary>
    public static ClaimsIdentity Identity
    {
        get
        {
            if (User.Identity is ClaimsIdentity identity)
                return identity;
            return UnauthenticatedIdentity.Instance;
        }
    }

    #endregion
}
