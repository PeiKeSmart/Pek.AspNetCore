﻿using Microsoft.AspNetCore.StaticFiles;

using NewLife;

namespace Pek;

public partial class WebHelperEx
{
    private static readonly IHttpContextAccessor _httpContextAccessor;

    static WebHelperEx()
    {
        _httpContextAccessor = Pek.Webs.HttpContext.Current.RequestServices.GetRequiredService<IHttpContextAccessor>();
    }

    /// <summary>
    /// 如果请求的资源是引擎不需要处理的典型资源之一，则返回true。
    /// </summary>
    /// <returns>如果请求针对静态资源文件，则为True。</returns>
    public static Boolean IsStaticResource(HttpContext? httpContext = null)
    {
        if (!IsRequestAvailable(httpContext))
            return false;

        var path = httpContext == null ? _httpContextAccessor.HttpContext?.Request.Path : httpContext.Request.Path;

        if (path == null)
        {
            return false;
        }

        var extension = GetExtension(path);
        if (extension == null)
        {
            return false;
        }

        var hashSet = new HashSet<String>
        {
            ".map",
            ".css"
        };
        if (hashSet.Contains(extension))
        {
            return true;
        }

        // 一些解决方法。 FileExtensionContentTypeProvider包含大多数静态文件扩展名。 所以我们可以使用它
        // 参考: https://github.com/aspnet/StaticFiles/blob/dev/src/Microsoft.AspNetCore.StaticFiles/FileExtensionContentTypeProvider.cs
        // 如果可以返回内容类型，则为静态文件
        var contentTypeProvider = new FileExtensionContentTypeProvider();

        return contentTypeProvider.TryGetContentType(path, out var _);
    }

    /// <summary>
    /// 检查当前HTTP请求是否可用
    /// </summary>
    /// <returns>如果可用，则为true；否则为true。 否则为假</returns>
    public static Boolean IsRequestAvailable(HttpContext? httpContext = null)
    {
        if (httpContext != null) return true;

        if (_httpContextAccessor?.HttpContext == null)
            return false;

        try
        {
            if (_httpContextAccessor.HttpContext.Request == null)
                return false;
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    private static String? GetExtension(String? path)
    {
        // Don't use Path.GetExtension as that may throw an exception if there are
        // invalid characters in the path. Invalid characters should be handled
        // by the FileProviders

        if (path.IsNullOrWhiteSpace())
        {
            return null;
        }

        var index = path.LastIndexOf('.');
        if (index < 0)
        {
            return null;
        }

        return path[index..];
    }
}
