using Microsoft.AspNetCore.ResponseCompression;

namespace Pek.Compression;

/// <summary>
/// 自定义压缩提供程序选择器
/// </summary>
public class BrowserCompatibleCompressionProvider : ICompressionProvider
{
    private readonly BrotliCompressionProvider _brotliProvider;
    private readonly GzipCompressionProvider _gzipProvider;
    private readonly HttpContext? _httpContext;

    public BrowserCompatibleCompressionProvider(
        BrotliCompressionProvider brotliProvider,
        GzipCompressionProvider gzipProvider,
        IHttpContextAccessor httpContextAccessor)
    {
        _brotliProvider = brotliProvider;
        _gzipProvider = gzipProvider;
        _httpContext = httpContextAccessor.HttpContext;
    }

    public String EncodingName =>
        SupportsBrotli() ? _brotliProvider.EncodingName : _gzipProvider.EncodingName;

    public Boolean SupportsFlush => true;

    public Stream CreateStream(Stream outputStream)
    {
        return SupportsBrotli()
            ? _brotliProvider.CreateStream(outputStream)
            : _gzipProvider.CreateStream(outputStream);
    }

    private Boolean SupportsBrotli()
    {
        if (_httpContext == null)
            return false;

        var acceptEncoding = _httpContext.Request.Headers["Accept-Encoding"].ToString().ToLowerInvariant();
        return acceptEncoding.Contains("br");
    }
}