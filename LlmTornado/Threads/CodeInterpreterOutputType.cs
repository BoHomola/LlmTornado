using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LlmTornado.Threads;

/// <summary>
/// Represents the type of output generated by a code interpreter.
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum CodeInterpreterOutputType
{
    /// <summary>
    /// Represents log output generated by a code interpreter.
    /// This member indicates that the output type is textual logs that provide
    /// information about the operations or actions performed by the code interpreter.
    /// </summary>
    [JsonProperty("logs")] Logs,

    /// <summary>
    /// Represents an output type where the generated output is an image.
    /// </summary>
    [JsonProperty("image")] Image
}