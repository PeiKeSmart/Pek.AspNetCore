﻿using System.IO.Compression;
using System.Text;

using NewLife;
using NewLife.Collections;
using NewLife.Log;
using NewLife.Serialization;

using Pek.Configs;
using Pek.Models;

namespace Pek.WebMiddleware;

/// <summary>
///  请求日志中间件
/// </summary>
/// <remarks>
/// 构造 请求日志中间件
/// </remarks>
/// <param name="next"></param>
public class RequestLoggerMiddleware(
    RequestDelegate next
        )
{
    private readonly RequestDelegate _next = next;

    /// <summary>
    /// 执行响应流指向新对象
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task Invoke(HttpContext context)
    {
        if (PekSysSetting.Current.AllowRequestParams)  // 允许获取则执行
        {
            if (context.Request.Path.Value?.Contains("/api/", StringComparison.OrdinalIgnoreCase) == false)
            {
                // 或请求管道中调用下一个中间件
                await _next(context).ConfigureAwait(false);
                return;
            }

            if (!PekSysSetting.Current.ExcludeUrl.IsNullOrWhiteSpace())  // 过滤指定路径
            {
                foreach (var item in PekSysSetting.Current.ExcludeUrl.Split(','))
                {
                    if (!item.IsNullOrWhiteSpace() && context.Request.Path.Value?.Contains(item, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        // 或请求管道中调用下一个中间件
                        await _next(context).ConfigureAwait(false);
                        return;
                    }
                }
            }

            XTrace.WriteLine($"[DHWeb.RequestLoggerMiddleware]Handling request: " + context.Request.Path);

            var api = new ApiRequestInputViewModel
            {
                HttpType = context.Request.Method,
                Query = context.Request.QueryString.Value,
                RequestUrl = context.Request.Path,
                RequestName = "",
                RequestIP = context.Request.Host.Value,
            };

            // 记录请求参数
            await LogRequest(context, api).ConfigureAwait(false);

            // 拦截响应流
            var originalResponseBodyStream = context.Response.Body;
            using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            // 处理请求
            await _next(context).ConfigureAwait(false);

            // 记录响应详情
            await LogResponse(context, responseBodyStream, api).ConfigureAwait(false);

            // 重置原始响应体流
            await responseBodyStream.CopyToAsync(originalResponseBodyStream).ConfigureAwait(false);

            // 响应完成时存入缓存
            context.Response.OnCompleted(() =>
            {
                XTrace.WriteLine($"[DHWeb.RequestLoggerMiddleware]RequestLog:{DateTime.Now.ToString("yyyyMMddHHmmssfff") + (new Random()).Next(0, 10000)}-{api.ToJson()}");

                return Task.CompletedTask;
            });
        }
        else if (!PekSysSetting.Current.RequestParamsUrl.IsNullOrWhiteSpace())
        {
            var list = PekSysSetting.Current.RequestParamsUrl.Split(',', StringSplitOptions.RemoveEmptyEntries);
            if (list.All(item => item.IsNullOrWhiteSpace()) ||
                (list.Length != 0 && !list.Any(item => context.Request.Path.Value?.Contains(item, StringComparison.OrdinalIgnoreCase) == true)))
            {
                // 或请求管道中调用下一个中间件
                await _next(context).ConfigureAwait(false);
                return;
            }

            XTrace.WriteLine($"[DHWeb.RequestLoggerMiddleware]Handling request: " + context.Request.Path);

            var api = new ApiRequestInputViewModel
            {
                HttpType = context.Request.Method,
                Query = context.Request.QueryString.Value,
                RequestUrl = context.Request.Path,
                RequestName = "",
                RequestIP = context.Request.Host.Value,
            };

            // 记录请求参数
            await LogRequest(context, api).ConfigureAwait(false);

            // 拦截响应流
            var originalResponseBodyStream = context.Response.Body;
            using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            // 处理请求
            await _next(context).ConfigureAwait(false);

            // 记录响应详情
            await LogResponse(context, responseBodyStream, api).ConfigureAwait(false);

            // 重置原始响应体流
            await responseBodyStream.CopyToAsync(originalResponseBodyStream).ConfigureAwait(false);

            // 响应完成时存入缓存
            context.Response.OnCompleted(() =>
            {
                XTrace.WriteLine($"[DHWeb.RequestLoggerMiddleware]RequestLog:{DateTime.Now.ToString("yyyyMMddHHmmssfff") + (new Random()).Next(0, 10000)}-{api.ToJson()}");

                return Task.CompletedTask;
            });
        }
        else
        {
            // 或请求管道中调用下一个中间件
            await _next(context).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// 记录请求参数
    /// </summary>
    /// <param name="context"></param>
    /// <param name="api"></param>
    /// <returns></returns>
    private static async Task LogRequest(HttpContext context, ApiRequestInputViewModel api)
    {
        context.Request.EnableBuffering(); // 启用请求体缓冲

        var header = Pool.StringBuilder.Get();
        foreach (var item in context.Request.Headers)
        {
            header.Append($",{item.Key}={item.Value}");
        }
        if (header.Length > 0)
        {
            header = header.Remove(0, 1);
        }
        api.RequestHeader = header.Return(true);

        if (!context.Request.ContentType.IsNullOrWhiteSpace() && context.Request.ContentType.Contains("application/octet-stream"))
        {
            // 记录数据长度
            var contentLength = context.Request.ContentLength ?? 0;

            // 读取并打印十六进制数据
            if (contentLength > 0)
            {
                var buffer = new Byte[contentLength];
                await context.Request.Body.ReadAsync(buffer).ConfigureAwait(false);
                context.Request.Body.Position = 0;

                var hexString = BitConverter.ToString(buffer).Replace("-", " ");
                api.Body = hexString;
            }
        }
        else
        {
            var request = context.Request;
            var requestBody = String.Empty;

            if (request.ContentLength > 0 && request.ContentType != null)
            {
                // 处理文件上传请求 - multipart/form-data
                if (request.ContentType.Contains("multipart/form-data", StringComparison.OrdinalIgnoreCase) && request.HasFormContentType)
                {
                    try
                    {
                        var form = await request.ReadFormAsync().ConfigureAwait(false);
                        var formSummary = Pool.StringBuilder.Get();
                        
                        // 记录普通表单字段
                        foreach (var field in form)
                        {
                            if (field.Value.Count > 0)
                            {
                                formSummary.Append($"{field.Key}={field.Value.Join(",")};");
                            }
                        }
                        
                        // 记录文件信息（不记录文件内容）
                        if (form.Files.Count > 0)
                        {
                            formSummary.Append($"Files=[");
                            for (var i = 0; i < form.Files.Count; i++)
                            {
                                var file = form.Files[i];
                                if (i > 0) formSummary.Append(",");
                                formSummary.Append($"{{Name:{file.Name},FileName:{file.FileName},Size:{file.Length},ContentType:{file.ContentType}}}");
                            }
                            formSummary.Append("];");
                        }
                        
                        requestBody = formSummary.Return(true);
                    }
                    catch (Exception ex)
                    {
                        requestBody = $"[解析表单失败: {ex.Message}]";
                    }
                }
                else
                {
                    using var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true);
                    requestBody = await reader.ReadToEndAsync().ConfigureAwait(false);
                    context.Request.Body.Position = 0;
                }
            }

            api.Body = requestBody;
        }
    }

    /// <summary>
    /// 记录响应详情
    /// </summary>
    /// <param name="context"></param>
    /// <param name="responseBodyStream"></param>
    /// <param name="api"></param>
    /// <returns></returns>
    private static async Task LogResponse(HttpContext context, MemoryStream responseBodyStream, ApiRequestInputViewModel api)
    {
        context.Response.Body.Seek(0, SeekOrigin.Begin);

        // 处理下载文件流单独处理
        if (!context.Response.ContentType.IsNullOrWhiteSpace() && context.Response.ContentType.Contains("application/octet-stream"))
        {
            // 读取并打印十六进制数据
            var buffer = responseBodyStream.ToArray();
            var hexString = BitConverter.ToString(buffer).Replace("-", " ");
            api.ResponseBody = hexString;
        }
        else
        {
            var buffer = responseBodyStream.ToArray();
            var responseBody = String.Empty;
            
            // 检查是否有压缩编码
            if (context.Response.Headers.ContainsKey("Content-Encoding"))
            {
                var encoding = context.Response.Headers["Content-Encoding"].ToString().ToLowerInvariant();
                
                try
                {
                    using var compressedStream = new MemoryStream(buffer);
                    Stream decompressedStream = encoding switch
                    {
                        "gzip" => new GZipStream(compressedStream, CompressionMode.Decompress),
                        "deflate" => new DeflateStream(compressedStream, CompressionMode.Decompress),
                        "br" => new BrotliStream(compressedStream, CompressionMode.Decompress),
                        _ => compressedStream
                    };
                    
                    using (decompressedStream)
                    using (var reader = new StreamReader(decompressedStream, Encoding.UTF8))
                    {
                        responseBody = await reader.ReadToEndAsync().ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    // 如果解压缩失败，记录原始数据
                    responseBody = $"[解压缩失败: {ex.Message}] 原始数据长度: {buffer.Length} bytes";
                }
            }
            else
            {
                // 没有压缩，直接读取
                responseBody = Encoding.UTF8.GetString(buffer);
            }
            
            api.ResponseBody = responseBody;
        }

        context.Response.Body.Seek(0, SeekOrigin.Begin);
    }
}

/// <summary>
/// 请求日志中间件扩展
/// </summary>
public static class RequestLoggerMiddlewareExtensions
{
    /// <summary>
    /// 使用请求日志中间件
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IApplicationBuilder UseRequestLogger(this IApplicationBuilder app) => app == null ? throw new ArgumentNullException(nameof(app)) : app.UseMiddleware<RequestLoggerMiddleware>();
}
