using System;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Backend.Fx.AspNet.ErrorHandling;
using Backend.Fx.AspNet.Util;
using Backend.Fx.Exceptions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace Backend.Fx.AspNet.Tests;

public class TheJsonErrorHandlingMiddleware
{
    private readonly TestServer _server;

    public TheJsonErrorHandlingMiddleware()
    {
        var builder = new WebHostBuilder()
            .Configure(
                app =>
                {
                    app.UseJsonErrorHandlingMiddleware(showInternalServerErrorDetails: true);
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

    [Theory]
    [InlineData("text/plain")]
    [InlineData("*/*")]
    [InlineData("application/json")]
    public async Task DoesNothingWhenNoErrorHappens(string header)
    {
        using var client = _server.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(header));
        var response = await client.GetAsync("/ok");
        response.EnsureSuccessStatusCode();
        string content = await response.Content.ReadAsStringAsync();
        Assert.Equal("ok", content);
    }

    [Theory]
    [InlineData("*/*")]
    [InlineData("application/json")]
    public async Task HandlesClientExceptionsWhenRequestHasMatchingAcceptHeader(string header)
    {
        using var client = _server.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(header));
        var response = await client.GetAsync("/clientexception");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var errorResponse = await response.TryGetErrorResponse();

        Assert.NotNull(errorResponse);
        Assert.Single(errorResponse.Errors);
    }

    [Theory]
    [InlineData("*/*")]
    [InlineData("application/json")]
    public async Task HandlesArgumentExceptionsWhenRequestHasMatchingAcceptHeader(string header)
    {
        using var client = _server.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(header));
        var response = await client.GetAsync("/argumentexception");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("*/*")]
    [InlineData("application/json")]
    public async Task HandlesNotFoundExceptionsWhenRequestHasMatchingAcceptHeader(string header)
    {
        using var client = _server.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(header));
        var response = await client.GetAsync("/notfound");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Theory]
    [InlineData("*/*")]
    [InlineData("application/json")]
    public async Task HandlesAllOtherExceptionsAsServerErrorWhenRequestHasMatchingAcceptHeader(string header)
    {
        using var client = _server.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(header));
        var response = await client.GetAsync("/dividebyzeroexception");
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        string content = await response.Content.ReadAsStringAsync();
        Assert.Contains("message", content);
        Assert.Contains("stackTrace", content); // because showInternalServerErrorDetails: true
    }

    [Fact]
    public async Task DoesNotHandleWhenNoAcceptHeaderProvided()
    {
        using var client = _server.CreateClient();
        await Assert.ThrowsAsync<ClientException>(async () => await client.GetAsync("/clientexception"));
        await Assert.ThrowsAsync<DivideByZeroException>(async () => await client.GetAsync("/DivideByZeroException"));
    }
}
