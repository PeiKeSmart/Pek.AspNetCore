using System.ComponentModel;

using NewLife;
using NewLife.Serialization;

namespace Pek.Cookies;

public class Cookie : ICookie
{
    private readonly HttpContext? _httpContext;
    private const Single DefaultExpireDurationMinutes = 43200; // 1 month
    private const Boolean DefaultHttpOnly = true;
    private const Boolean ExpireWithBrowser = false;
    private const String DefaultPath = "/";

    public Cookie(IHttpContextAccessor httpContextAccessor)
    {
        _httpContext = httpContextAccessor.HttpContext;
    }

    public T? GetValue<T>(String name) => GetValue<T>(name, false);

    public T? GetValue<T>(String name, Boolean expireOnceRead)
    {
        T? value = default;

        if (_httpContext?.Request.Cookies.TryGetValue(name, out var valuStr) == true)
        {
            if (!valuStr.IsNullOrWhiteSpace())
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));

                try
                {
                    value = (T?)converter.ConvertFromString(valuStr);
                }
                catch (NotSupportedException)
                {
                    try
                    {
                        value = JsonHelper.ToJsonEntity<T>(valuStr);
                    }
                    catch (NotSupportedException)
                    {
                        if (converter.CanConvertFrom(typeof(String)))
                        {
                            value = (T?)converter.ConvertFrom(valuStr);
                        }
                    }
                }
            }

            if (expireOnceRead)
            {
                Delete(name);
            }
        }

        return value;
    }

    public void SetValue<T>(String name, T value)
    {
        SetValue(name, value, DefaultExpireDurationMinutes, DefaultHttpOnly, ExpireWithBrowser, DefaultPath);
    }

    public void SetValue<T>(String name, T value, String path)
    {
        SetValue(name, value, DefaultExpireDurationMinutes, DefaultHttpOnly, ExpireWithBrowser, path);
    }

    public void SetValue<T>(String name, T value, Single expireDurationInMinutes)
    {
        SetValue(name, value, expireDurationInMinutes, DefaultHttpOnly, ExpireWithBrowser, DefaultPath);
    }

    public void SetValue<T>(String name, T value, Single expireDurationInMinutes, String path)
    {
        SetValue(name, value, expireDurationInMinutes, DefaultHttpOnly, ExpireWithBrowser, path);
    }

    public void SetValue<T>(String name, T value, Boolean httpOnly, Boolean expireWithBrowser)
    {
        SetValue(name, value, DefaultExpireDurationMinutes, httpOnly, expireWithBrowser, DefaultPath);
    }

    public void SetValue<T>(String name, T value, Boolean httpOnly, Boolean expireWithBrowser, String path)
    {
        SetValue(name, value, DefaultExpireDurationMinutes, httpOnly, expireWithBrowser, path);
    }

    public void SetValue<T>(String name, T value, Single expireDurationInMinutes, Boolean httpOnly, Boolean expireWithBrowser, String path)
    {
        var converter = TypeDescriptor.GetConverter(typeof(T));

        var cookieValue = String.Empty;

        try
        {
            cookieValue = converter.ConvertToString(value);
        }
        catch (NotSupportedException)
        {
            if (converter.CanConvertTo(typeof(String)))
            {
                cookieValue = (String?)converter.ConvertTo(value, typeof(String));
            }
        }

        if (!cookieValue.IsNullOrWhiteSpace())
        {

            if (expireWithBrowser)
            {

                _httpContext?.Response.Cookies.Append(name, cookieValue);
            }
            else
            {
                _httpContext?.Response.Cookies.Append(name, cookieValue, new CookieOptions
                {
                    Expires = DateTime.Now.AddMinutes(expireDurationInMinutes),
                    HttpOnly = httpOnly,
                    Path = path,
                    SameSite = SameSiteMode.None
                });
            }

        }
    }

    public void Delete(String name) => _httpContext?.Response.Cookies.Append(name, "", new CookieOptions { Expires = DateTime.Now.AddDays(-1d) });
}