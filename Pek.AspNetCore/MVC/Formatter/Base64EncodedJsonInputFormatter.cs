﻿using System.Text;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;

namespace Pek.MVC.Formatter;

/// <summary>
/// body 支持 简单的 base64 编码
/// </summary>
/// <remarks>https://mp.weixin.qq.com/s/rolYK24D_dHEEZgKyVPM6w</remarks>
public class Base64EncodedJsonInputFormatter : TextInputFormatter
{
    public Base64EncodedJsonInputFormatter()
    {
        // 注册支持的 Content-Type
        SupportedMediaTypes.Add("text/base64-json");
        SupportedEncodings.Add(Encoding.UTF8);
    }

    public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context, Encoding encoding)
    {
        try
        {
            using var reader = context.ReaderFactory(context.HttpContext.Request.Body, encoding);
            var rawContent = await reader.ReadToEndAsync().ConfigureAwait(false);
            if (String.IsNullOrEmpty(rawContent))
            {
                return await InputFormatterResult.NoValueAsync().ConfigureAwait(false);
            }
            var bytes = Convert.FromBase64String(rawContent);
            var services = context.HttpContext.RequestServices;

            var modelValue = await GetModelValue(services, bytes).ConfigureAwait(false);
            return await InputFormatterResult.SuccessAsync(modelValue).ConfigureAwait(false);

            async ValueTask<Object> GetModelValue(IServiceProvider serviceProvider, Byte[] stringBytes)
            {
                var newtonJsonOption = serviceProvider.GetService<IOptions<MvcNewtonsoftJsonOptions>>()?.Value;
                if (newtonJsonOption is null)
                {
                    using var stream = new MemoryStream(stringBytes);
                    var result = await System.Text.Json.JsonSerializer.DeserializeAsync(stream, context.ModelType,
                        services.GetRequiredService<IOptions<JsonOptions>>().Value.JsonSerializerOptions).ConfigureAwait(false);
                    return result!;
                }

                var stringContent = encoding.GetString(bytes);
                return Newtonsoft.Json.JsonConvert.DeserializeObject(stringContent, context.ModelType, newtonJsonOption.SerializerSettings)!;
            }
        }
        catch (Exception e)
        {
            context.ModelState.TryAddModelError(String.Empty, e.Message);
            return await InputFormatterResult.FailureAsync().ConfigureAwait(false);
        }
    }
}