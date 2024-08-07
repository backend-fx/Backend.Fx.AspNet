﻿using System;
using Backend.Fx.Exceptions;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.AspNet.Mvc.Throttling;

/// <summary>
/// returns HTTP 429 "Too many requests" when the attributed action gets called from the same IP address in less than
/// the configured interval and an exception was thrown. Useful to prevent brute force attacks..
/// </summary>
[PublicAPI]
public class ExceptionThrottlingAttribute : ThrottlingBaseAttribute
{
    public ExceptionThrottlingAttribute(string name) : base(name)
    {
    }

    public override void OnActionExecuted(ActionExecutedContext actionContext)
    {
        var cache = actionContext.HttpContext.RequestServices.GetRequiredService<IMemoryCache>();
        string key = string.Concat(Name, "-", actionContext.HttpContext.Connection.RemoteIpAddress);

        if (actionContext.Exception == null)
        {
            cache.Remove(key);
            return;
        }

        if (cache.TryGetValue(key, out int repetition))
        {
            int retryAfter = Math.Max(1, CalculateRepeatedTimeoutFactor(repetition)) * Seconds;
            cache.Set(key, ++repetition, TimeSpan.FromSeconds(retryAfter));
            throw new TooManyRequestsException(retryAfter).AddError(string.Format(Message, retryAfter));
        }

        cache.Set(key, 1, TimeSpan.FromSeconds(Seconds));
    }

    protected override int CalculateRepeatedTimeoutFactor(int repetition)
    {
        return repetition * repetition;
    }
}
