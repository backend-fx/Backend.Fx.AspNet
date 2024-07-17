using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Backend.Fx.AspNet.ErrorHandling;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace Backend.Fx.AspNet.Util;

[PublicAPI]
public static class HttpResponseEx
{
    public static async Task WriteJsonAsync(
        this HttpResponse response,
        object o,
        JsonSerializerOptions? options = null,
        string? contentType = null)
    {
        options ??= new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            AllowTrailingCommas = true,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
        };

        await response.WriteJsonAsync(JsonSerializer.Serialize(o, options), contentType);
    }

    public static async Task WriteJsonAsync(this HttpResponse response, string json, string? contentType = null)
    {
        response.ContentType = contentType ?? "application/json; charset=UTF-8";
        await response.WriteAsync(json);
        await response.Body.FlushAsync();
    }

    public static async Task<ErrorResponse?> TryGetErrorResponse(this HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return null;
        }

        try
        {
            return await response.Content.ReadFromJsonAsync<ErrorResponse>();
        }
        catch
        {
            return null;
        }
    }
}
