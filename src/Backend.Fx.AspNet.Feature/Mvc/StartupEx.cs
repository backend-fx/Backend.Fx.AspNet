using Backend.Fx.Execution;
using Backend.Fx.Execution.Pipeline;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.AspNet.Mvc;

[PublicAPI]
public static class StartupEx
{
    public static void AddBackendFxMvcApplication<TBackendFxApplication>(
        this IServiceCollection services,
        TBackendFxApplication application)
        where TBackendFxApplication : IBackendFxApplication
    {
        application.EnableFeature(new AspNetMvcFeature(services));
    }

    public static void UseBackendFxMvcApplication<TBackendFxApplication>(
        this IApplicationBuilder app,
        TBackendFxApplication application)
        where TBackendFxApplication : IBackendFxApplication
    {
        app.Use(async (context, requestDelegate) =>
        {
            await application
                .Invoker
                .InvokeAsync(
                    (_, _) => requestDelegate.Invoke(),
                    context.User.Identity ?? new AnonymousIdentity(),
                    context.RequestAborted)
                .ConfigureAwait(false);
        });
    }
}