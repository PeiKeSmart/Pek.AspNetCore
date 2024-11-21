namespace Pek.Cookies;

public interface ICookie
{
    T? GetValue<T>(String name);

    T? GetValue<T>(String name, Boolean expireOnceRead);

    void SetValue<T>(String name, T value);

    void SetValue<T>(String name, T value, String path);

    /// <summary>
    /// 设置Cookie项
    /// </summary>
    /// <typeparam name="T">对象</typeparam>
    /// <param name="name">键</param>
    /// <param name="value">值</param>
    /// <param name="expireDurationInMinutes">过期时间，分钟。</param>
    void SetValue<T>(String name, T value, Single expireDurationInMinutes);

    void SetValue<T>(String name, T value, Single expireDurationInMinutes, String path);

    void SetValue<T>(String name, T value, Boolean httpOnly, Boolean expireWithBrowser);

    void SetValue<T>(String name, T value, Boolean httpOnly, Boolean expireWithBrowser, String path);

    void SetValue<T>(String name, T value, Single expireDurationInMinutes, Boolean httpOnly, Boolean expireWithBrowser, String Path);

    void Delete(String name);
}