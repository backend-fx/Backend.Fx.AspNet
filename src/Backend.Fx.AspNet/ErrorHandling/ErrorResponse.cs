using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
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
            .FirstOrDefault() ?? string.Empty;

        Errors = errors
            .Where(kvp => kvp.Key != Backend.Fx.Exceptions.Errors.GenericErrorKey)
            .Select(kvp => new SerializableError { Key = kvp.Key, Errors = kvp.Value })
            .ToArray();
    }

    [JsonPropertyName("_error")]
    public string GenericError { get; set; } = string.Empty;

    [JsonPropertyName("errors")]
    public SerializableError[] Errors { get; set; } = Array.Empty<SerializableError>();

    public string ToJsonString(JsonSerializerOptions? options = null)
    {
        options ??= new JsonSerializerOptions { WriteIndented = true };
        return JsonSerializer.Serialize(this, options);
    }
}
