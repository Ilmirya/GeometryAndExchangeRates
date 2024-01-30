using Microsoft.AspNetCore.Http.Extensions;

namespace GeometryAndExchangeRates.Middlewares;

public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;
    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var requestUrl = context.Request.GetDisplayUrl();
        var requestMethod = context.Request.Method;
        var requestBody = await GetBodyString(context.Request.Body);

        _logger.LogInformation($"Request: Url:{requestUrl}: Method:{requestMethod}; Body:{requestBody}.");
        await _next(context);
    }
    
    
    private static async Task<string> GetBodyString(Stream requestBody)
    {
        if (!requestBody.CanSeek || !requestBody.CanRead)
        {
            return "<<-->>";
        }

        try
        {
            using var streamReader = new StreamReader(requestBody, leaveOpen: true);
            var bodyString = await streamReader.ReadToEndAsync();
            return bodyString;
        }
        catch (Exception ex)
        {
            return $"<<Ошибка чтения тела запроса: {ex.Message}>>";
        }
    }
}