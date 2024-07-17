using System;
using System.Threading.Tasks;
using Backend.Fx.AspNet.ErrorHandling;
using Backend.Fx.Exceptions;
using Backend.Fx.Logging;
using FakeItEasy;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace Backend.Fx.AspNet.Tests;

public class TheErrorLoggingMiddleware
{
    private readonly TestServer _server;

    private readonly IExceptionLogger _exceptionLogger = A.Fake<IExceptionLogger>();

    public TheErrorLoggingMiddleware()
    {
        var builder = new WebHostBuilder()
            .Configure(
                app =>
                {
                    app.UseErrorLoggingMiddleware(_exceptionLogger);
                    app.Map("/argumentexception", b => b.Run(_ => throw new ArgumentException("test")));
                    app.Map("/DivideByZeroException", b => b.Run(_ => throw new DivideByZeroException()));
                    app.Map(
                        "/clientexception", b => b.Run(
                                                _ => throw new ClientException("test")
                                                    .AddError("key", "error")));
                    app.Map("/notfund", b => b.Run(_ => throw new NotFoundException()));
                    app.Map(
                        "/ok",
                        b => b.Run(
                            async context =>
                            {
                                context.Response.StatusCode = 200;
                                await context.Response.WriteAsync("ok");
                            }));
                });

        _server = new TestServer(builder);
    }

    [Fact]
    public async Task DoesNothingWhenNoErrorHappens()
    {
        using var client = _server.CreateClient();
        var response = await client.GetAsync("/ok");
        response.EnsureSuccessStatusCode();
        string content = await response.Content.ReadAsStringAsync();
        Assert.Equal("ok", content);

        A.CallTo(() => _exceptionLogger.LogException(A<Exception>._)).MustNotHaveHappened();
    }

    [Fact]
    public async Task LogsClientExceptions()
    {
        using var client = _server.CreateClient();
        await Assert.ThrowsAsync<ClientException>(async () =>  await client.GetAsync("/clientexception"));
        A.CallTo(() => _exceptionLogger.LogException(A<ClientException>._)).MustHaveHappenedOnceExactly();
    }

    [Fact]
    public async Task LogsAllOtherExceptions()
    {
        using var client = _server.CreateClient();
        await Assert.ThrowsAsync<DivideByZeroException>(async () =>  await client.GetAsync("/dividebyzeroexception"));
        A.CallTo(() => _exceptionLogger.LogException(A<DivideByZeroException>._)).MustHaveHappenedOnceExactly();
    }
}
