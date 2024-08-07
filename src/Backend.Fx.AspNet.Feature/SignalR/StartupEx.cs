using Backend.Fx.Execution;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.AspNet.SignalR;

[PublicAPI]
public static class StartupEx
{
    public static void AddBackendFxSignalRApplication(
        this IServiceCollection services,
        IBackendFxApplication application)
    {
        application.EnableFeature(new AspNetSignalRFeature(services));
    }
}
