using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;

namespace Backend.Fx.AspNet.ErrorHandling;

[PublicAPI]
public static class ErrorHandlingExtensions
{
    public static IApplicationBuilder UseJsonErrorHandlingMiddleware(
        this IApplicationBuilder builder,
        bool showInternalServerErrorDetails = false)
    {
        return builder.UseMiddleware<JsonErrorHandlingMiddleware>(showInternalServerErrorDetails);
    }

    public static IApplicationBuilder UseErrorLoggingMiddleware(
        this IApplicationBuilder builder,
        IExceptionLogger exceptionLogger)
    {
        return builder.UseMiddleware<ErrorLoggingMiddleware>(exceptionLogger);
    }
}
