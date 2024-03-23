using System.Net;
using System.Threading.Tasks;
using Backend.Fx.Execution;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Backend.Fx.AspNet;

public class WaitForBootMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IBackendFxApplication _application;

    public WaitForBootMiddleware(RequestDelegate next, IBackendFxApplication application)
    {
        _next = next;
        _application = application;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (_application.State is BackendFxApplicationState.Initializing or BackendFxApplicationState.Booting)
        {
            await _application.WaitForBootAsync(context.RequestAborted);
            await _next.Invoke(context);
        }

        if (_application.State == BackendFxApplicationState.Booted)
        {
            await _next.Invoke(context);
        }
        else
        {
            context.Response.StatusCode = (int)HttpStatusCode.ServiceUnavailable;
            await context.Response.WriteAsync(
                "Service Unavailable - please try again later",
                cancellationToken: context.RequestAborted);
        }
    }
}

public static class WaitForBootMiddlewareExtensions
{
    public static IApplicationBuilder UseWaitForBootMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<WaitForBootMiddleware>();
    }
}