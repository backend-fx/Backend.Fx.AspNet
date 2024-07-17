using System;
using System.Collections.Generic;
using System.Linq;
using Backend.Fx.Exceptions;
using Backend.Fx.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace Backend.Fx.AspNet.Mvc.Validation;

public abstract class ModelValidationFilter : IActionFilter
{
    public abstract void OnActionExecuting(ActionExecutingContext context);
    public abstract void OnActionExecuted(ActionExecutedContext context);

    protected static void LogErrors(FilterContext context, string controllerName, Errors errors)
    {
        var logger = TryGetControllerType(controllerName, out var controllerType)
            ? Log.Create(controllerType)
            : Log.Create<ModelValidationFilter>();
        logger.LogWarning(
            "Model validation failed during {Method} {RequestUrl}: {@Errors}",
            context.HttpContext.Request.Method,
            context.HttpContext.Request.GetDisplayUrl(),
            errors);
    }

    protected static bool AcceptsJson(FilterContext context)
    {
        IList<MediaTypeHeaderValue> accept = context.HttpContext.Request.GetTypedHeaders().Accept;
        return accept.Any(mth => mth.MatchesMediaType("application/json"));
    }

    protected static bool AcceptsHtml(FilterContext context)
    {
        IList<MediaTypeHeaderValue> accept = context.HttpContext.Request.GetTypedHeaders().Accept;
        return accept.Any(mth => mth.MatchesMediaType("text/html"));
    }

    private static bool TryGetControllerType(string controllerName, out Type type)
    {
        Type? maybeNullType;
        try
        {
            maybeNullType = Type.GetType(controllerName);
        }
        catch
        {
            maybeNullType = null!;
        }

        type = maybeNullType!;
        return maybeNullType != null;
    }
}
