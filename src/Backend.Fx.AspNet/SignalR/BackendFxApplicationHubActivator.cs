using System;
using Backend.Fx.Execution.DependencyInjection;
using Backend.Fx.Logging;
using JetBrains.Annotations;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.AspNet.SignalR;

[PublicAPI]
public class BackendFxApplicationHubActivator<T> : IHubActivator<T> where T : Hub
{
    private readonly ICompositionRoot _compositionRoot;
    private readonly ILogger _logger = Log.Create<BackendFxApplicationHubActivator<T>>();

    public BackendFxApplicationHubActivator(ICompositionRoot compositionRoot)
    {
        _compositionRoot = compositionRoot;
    }

    public T Create()
    {
        return _compositionRoot.ServiceProvider.GetRequiredService<T>();
    }

    public void Release(T hub)
    {
        _logger.LogTrace("Releasing {HubTypeName}", hub.GetType().Name);
        if (hub is IDisposable disposable)
        {
            _logger.LogDebug("Disposing {HubTypeName}", hub.GetType().Name);
            disposable.Dispose();
        }
    }
}
