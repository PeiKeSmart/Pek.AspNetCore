using System.ComponentModel;

using NewLife;
using NewLife.Serialization;

namespace Pek.Cookies;

public class Cookie : ICookie
{
    private readonly HttpContext _httpContext;
    private const float DefaultExpireDurationMinutes = 43200; // 1 month
    private const bool DefaultHttpOnly = true;
    private const bool ExpireWithBrowser = false;
    private const String DefaultPath = "/";

    public Cookie(IHttpContextAccessor httpContextAccessor)
    {
        _httpContext = httpContextAccessor.HttpContext;
    }

    public T GetValue<T>(string name)
    {
        return GetValue<T>(name, false);
    }

    public T GetValue<T>(string name, bool expireOnceRead)
    {
        T value = default;

        if (_httpContext.Request.Cookies.TryGetValue(name, out var valuStr))
        {
            if (!valuStr.IsNullOrWhiteSpace())
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));

                try
                {
                    value = (T)converter.ConvertFromString(valuStr);
                }
                catch (NotSupportedException)
                {
                    try
                    {
                        value = JsonHelper.ToJsonEntity<T>(valuStr);
                    }
                    catch (NotSupportedException)
                    {
                        if (converter.CanConvertFrom(typeof(string)))
                        {
                            value = (T)converter.ConvertFrom(valuStr);
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

    public void SetValue<T>(string name, T value)
    {
        SetValue(name, value, DefaultExpireDurationMinutes, DefaultHttpOnly, ExpireWithBrowser, DefaultPath);
    }

    public void SetValue<T>(string name, T value, String path)
    {
        SetValue(name, value, DefaultExpireDurationMinutes, DefaultHttpOnly, ExpireWithBrowser, path);
    }

    public void SetValue<T>(string name, T value, float expireDurationInMinutes)
    {
        SetValue(name, value, expireDurationInMinutes, DefaultHttpOnly, ExpireWithBrowser, DefaultPath);
    }

    public void SetValue<T>(string name, T value, float expireDurationInMinutes, String path)
    {
        SetValue(name, value, expireDurationInMinutes, DefaultHttpOnly, ExpireWithBrowser, path);
    }

    public void SetValue<T>(string name, T value, bool httpOnly, bool expireWithBrowser)
    {
        SetValue(name, value, DefaultExpireDurationMinutes, httpOnly, expireWithBrowser, DefaultPath);
    }

    public void SetValue<T>(string name, T value, bool httpOnly, bool expireWithBrowser, String path)
    {
        SetValue(name, value, DefaultExpireDurationMinutes, httpOnly, expireWithBrowser, path);
    }

    public void SetValue<T>(string name, T value, float expireDurationInMinutes, bool httpOnly, bool expireWithBrowser, String path)
    {
        var converter = TypeDescriptor.GetConverter(typeof(T));

        var cookieValue = string.Empty;

        try
        {
            cookieValue = converter.ConvertToString(value);
        }
        catch (NotSupportedException)
        {
            if (converter.CanConvertTo(typeof(string)))
            {
                cookieValue = (string)converter.ConvertTo(value, typeof(string));
            }
        }

        if (!cookieValue.IsNullOrWhiteSpace())
        {

            if (expireWithBrowser)
            {

                _httpContext.Response.Cookies.Append(name, cookieValue);
            }
            else
            {
                _httpContext.Response.Cookies.Append(name, cookieValue, new CookieOptions
                {
                    Expires = DateTime.Now.AddMinutes(expireDurationInMinutes),
                    HttpOnly = httpOnly,
                    Path = path
                });
            }

        }
    }

    public void Delete(string name)
    {
        _httpContext.Response.Cookies.Append(name, "", new CookieOptions { Expires = DateTime.Now.AddDays(-1d) });
    }
}