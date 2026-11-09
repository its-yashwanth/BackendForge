using Microsoft.AspNetCore.Diagnostics;

namespace MyFirstApi.Handlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    
    private readonly ILogger<GlobalExceptionHandler> _logger;
    
    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger=logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {

         _logger.LogError(
            exception,
            "An unhandled exception occurred while processing the request.");

        httpContext.Response.StatusCode = 500;

        await httpContext.Response.WriteAsJsonAsync(new
        {
            message = "An unexpected error occurred."
        }, cancellationToken);

        return true;
    }
}