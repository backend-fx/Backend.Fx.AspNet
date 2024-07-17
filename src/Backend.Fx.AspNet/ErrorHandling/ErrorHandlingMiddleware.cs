using System;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using Backend.Fx.Exceptions;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace Backend.Fx.AspNet.ErrorHandling;

public abstract class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>
    ///     This constructor is being called by the framework DI container
    /// </summary>
    [UsedImplicitly]
    protected ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    ///     This method is being called by the previous middleware in the HTTP pipeline
    /// </summary>
    [UsedImplicitly]
    public async Task Invoke(HttpContext context)
    {
        if (await ShouldHandle(context))
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (TooManyRequestsException tooManyRequestsException)
            {
                if (tooManyRequestsException.RetryAfter > 0)
                {
                    context.Response.Headers.Add(
                        "Retry-After",
                        tooManyRequestsException.RetryAfter.ToString(CultureInfo.InvariantCulture));
                }

                await HandleClientError(
                    context,
                    (int)HttpStatusCode.TooManyRequests,
                    HttpStatusCode.TooManyRequests.ToString(),
                    tooManyRequestsException);
            }
            catch (UnprocessableException uex)
            {
                await HandleClientError(
                    context,
                    (int)HttpStatusCode.UnprocessableEntity,
                    HttpStatusCode.UnprocessableEntity.ToString(),
                    uex);
            }
            catch (NotFoundException notFoundException)
            {
                await HandleClientError(
                    context,
                    (int)HttpStatusCode.NotFound,
                    HttpStatusCode.NotFound.ToString(),
                    notFoundException);
            }
            catch (ConflictedException conflictedException)
            {
                await HandleClientError(
                    context,
                    (int)HttpStatusCode.Conflict,
                    HttpStatusCode.Conflict.ToString(),
                    conflictedException);
            }
            catch (ForbiddenException forbiddenException)
            {
                await HandleClientError(
                    context,
                    (int)HttpStatusCode.Forbidden,
                    HttpStatusCode.Forbidden.ToString(),
                    forbiddenException);
            }
            catch (UnauthorizedException unauthorizedException)
            {
                await HandleClientError(
                    context,
                    (int)HttpStatusCode.Unauthorized,
                    HttpStatusCode.Unauthorized.ToString(),
                    unauthorizedException);
            }
            catch (ClientException clientException)
            {
                await HandleClientError(
                    context,
                    (int)HttpStatusCode.BadRequest,
                    HttpStatusCode.BadRequest.ToString(),
                    clientException);
            }
            catch (ArgumentException argumentException)
            {
                await HandleClientError(
                    context,
                    (int)HttpStatusCode.BadRequest,
                    argumentException.Message,
                    argumentException);
            }
            catch (Exception exception)
            {
                await HandleServerError(context, exception);
            }
        }
        else
        {
            await _next.Invoke(context);
        }
    }

    protected abstract Task<bool> ShouldHandle(HttpContext context);

    protected abstract Task HandleClientError(
        HttpContext context,
        int httpStatusCode,
        string message,
        Exception exception);

    protected abstract Task HandleServerError(HttpContext context, Exception exception);
}
