using System;
using System.Threading.Tasks;
using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace Backend.Fx.AspNet.ErrorHandling;

[PublicAPI]
public class ErrorLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IExceptionLogger _exceptionLogger;

    [UsedImplicitly]
    public ErrorLoggingMiddleware(RequestDelegate next, IExceptionLogger exceptionLogger)
    {
        _next = next;
        _exceptionLogger = exceptionLogger;
    }

    [UsedImplicitly]
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next.Invoke(context);
        }
        catch (Exception exception)
        {
            if (!context.Items.ContainsKey("ExceptionLogged"))
            {
                _exceptionLogger.LogException(exception);
                context.Items["ExceptionLogged"] = true;
            }

            throw;
        }
    }
}
