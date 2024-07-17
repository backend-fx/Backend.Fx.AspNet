using System;
using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Backend.Fx.AspNet.ErrorHandling;

[PublicAPI]
public class SerializableError
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("errors")]
    public string[] Errors { get; set; } = Array.Empty<string>();
}
