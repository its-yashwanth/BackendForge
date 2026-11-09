using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MyFirstApi.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;


    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        _logger.LogInformation(
            "Incoming request: {Method} {Path}",
            context.Request.Method,
            context.Request.Path
        );
        Console.WriteLine("before calling next middleware");
        
        await _next(context);

        Console.WriteLine("after calling next middleware");
        
    }
}