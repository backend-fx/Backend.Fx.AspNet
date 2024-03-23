using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;

namespace Backend.Fx.AspNet.ErrorHandling;

[PublicAPI]
public static class ErrorHandlingExtensions
{
    public static IApplicationBuilder UseJsonErrorHandlingMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<JsonErrorHandlingMiddleware>();
    }

    public static IApplicationBuilder UseErrorLoggingMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ErrorLoggingMiddleware>();
    }
}