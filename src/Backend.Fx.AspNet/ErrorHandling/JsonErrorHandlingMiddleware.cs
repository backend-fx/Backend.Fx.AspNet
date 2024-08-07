using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Backend.Fx.Exceptions;
using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace Backend.Fx.AspNet.ErrorHandling;

[PublicAPI]
public class JsonErrorHandlingMiddleware : ErrorHandlingMiddleware
{
    private readonly bool _showInternalServerErrorDetails;
    private static readonly ILogger Logger = Log.Create<JsonErrorHandlingMiddleware>();

    protected JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public JsonErrorHandlingMiddleware(RequestDelegate next, bool showInternalServerErrorDetails) : base(next)
    {
        _showInternalServerErrorDetails = showInternalServerErrorDetails;
    }

    protected override Task<bool> ShouldHandle(HttpContext context)
    {
        // this middleware only handles requests that accept json as response
        IList<MediaTypeHeaderValue> accept = context.Request.GetTypedHeaders().Accept;
        return Task.FromResult(accept.Any(mth => mth.MatchesMediaType("application/json")));
    }

    protected override async Task HandleClientError(
        HttpContext context,
        int httpStatusCode,
        string message,
        Exception exception)
    {
        if (context.Response.HasStarted)
        {
            Logger.LogWarning("exception cannot be handled correctly, because the response has already started");
            return;
        }

        // convention: only the errors array will be transmitted to the client, allowing technical (possibly
        // revealing) information in the exception message.
        var clientException = exception as ClientException;
        var errors = clientException?.HasErrors() == true
            ? clientException.Errors
            : new Errors($"HTTP{httpStatusCode}: {message}");

        context.Response.StatusCode = httpStatusCode;
        string serializedErrors = SerializeErrors(errors);
        context.Response.ContentType = "application/json; charset=utf-8";
        await context.Response.WriteAsync(serializedErrors);
    }

    protected override async Task HandleServerError(HttpContext context, Exception exception)
    {
        if (context.Response.HasStarted)
        {
            Logger.LogWarning("exception cannot be handled correctly, because the response has already started");
            return;
        }

        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        string responseContent = _showInternalServerErrorDetails
            ? JsonSerializer.Serialize(
                new { message = exception.Message, stackTrace = exception.StackTrace },
                JsonSerializerOptions)
            : JsonSerializer.Serialize(new { message = "An internal error occurred" }, JsonSerializerOptions);

        context.Response.ContentType = "application/json; charset=utf-8";
        await context.Response.WriteAsync(responseContent);
    }

    protected virtual string SerializeErrors(Errors errors)
    {
        return new ErrorResponse(errors).ToJsonString();
    }
}
