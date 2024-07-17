using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Backend.Fx.Exceptions;
using JetBrains.Annotations;

namespace Backend.Fx.AspNet.ErrorHandling;

[PublicAPI]
public class ErrorResponse
{
    public ErrorResponse()
    {
    }

    public ErrorResponse([NotNull] Errors errors)
    {
        if (errors == null) throw new ArgumentNullException(nameof(errors));

        GenericError = errors
            .Where(kvp => kvp.Key == Backend.Fx.Exceptions.Errors.GenericErrorKey)
            .Select(kvp => string.Join(Environment.NewLine, kvp.Value))
            .FirstOrDefault();

        Errors = errors
            .Where(kvp => kvp.Key != Backend.Fx.Exceptions.Errors.GenericErrorKey)
            .Select(kvp => new SerializableError { Key = kvp.Key, Errors = kvp.Value })
            .ToArray();
    }

    [JsonPropertyName("_error")]
    public string GenericError { get; set; } = string.Empty;

    [JsonPropertyName("errors")]
    public SerializableError[] Errors { get; set; } = Array.Empty<SerializableError>();

    public string ToJsonString(JsonSerializerOptions options = null)
    {
        options ??= new JsonSerializerOptions { WriteIndented = true };
        return JsonSerializer.Serialize(this, options);
    }
}


public static class ErrorResponseExtensions
{
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
