using Backend.Fx.Execution;
using Backend.Fx.Execution.Features;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.AspNet.SignalR;

[PublicAPI]
public class AspNetSignalRFeature : Feature
{
    private readonly IServiceCollection _frameworkServices;

    public AspNetSignalRFeature(IServiceCollection frameworkServices)
    {
        _frameworkServices = frameworkServices;
    }

    public override void Enable(IBackendFxApplication application)
    {
        application.CompositionRoot.RegisterModules(
            new AspNetSignalRModule(_frameworkServices, application.Assemblies));
    }
}
