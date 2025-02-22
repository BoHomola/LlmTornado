using System.Collections.Generic;
using LlmTornado.Chat.Models;
using LlmTornado.Common;
using Newtonsoft.Json;

namespace LlmTornado.Threads;

public sealed class CreateThreadAndRunRequest
{
    public CreateThreadAndRunRequest(string assistantId, CreateThreadAndRunRequest request)
        : this(assistantId, request?.Model, request?.Instructions, request?.Tools, request?.Metadata)
    {
    }

    /// <summary>
    ///     Constructor.
    /// </summary>
    /// <param name="assistantId">
    ///     The ID of the assistant to use to execute this run.
    /// </param>
    /// <param name="model">
    ///     The ID of the Model to be used to execute this run.
    ///     If a value is provided here, it will override the model associated with the assistant.
    ///     If not, the model associated with the assistant will be used.
    /// </param>
    /// <param name="instructions">
    ///     Override the default system message of the assistant.
    ///     This is useful for modifying the behavior on a per-run basis.
    /// </param>
    /// <param name="tools">
    ///     Override the tools the assistant can use for this run.
    ///     This is useful for modifying the behavior on a per-run basis.
    /// </param>
    /// <param name="metadata">
    ///     Set of 16 key-value pairs that can be attached to an object.
    ///     This can be useful for storing additional information about the object in a structured format.
    ///     Keys can be a maximum of 64 characters long and values can be a maximum of 512 characters long.
    /// </param>
    /// <param name="createThreadRequest">
    ///     Optional, <see cref="CreateThreadRequest" />.
    /// </param>
    public CreateThreadAndRunRequest(string assistantId, ChatModel? model = null, string? instructions = null, IReadOnlyList<Tool>? tools = null, IReadOnlyDictionary<string, string>? metadata = null, CreateThreadRequest? createThreadRequest = null)
    {
        AssistantId = assistantId;
        Model = model;
        Instructions = instructions;
        Tools = tools;
        Metadata = metadata;
        ThreadRequest = createThreadRequest;
    }

    /// <summary>
    ///     The ID of the assistant to use to execute this run.
    /// </summary>
    [JsonProperty("assistant_id")]
    public string AssistantId { get; }

    /// <summary>
    ///     The ID of the Model to be used to execute this run.
    ///     If a value is provided here, it will override the model associated with the assistant.
    ///     If not, the model associated with the assistant will be used.
    /// </summary>
    [JsonProperty("model")]
    public string Model { get; }

    /// <summary>
    ///     Override the default system message of the assistant.
    ///     This is useful for modifying the behavior on a per-run basis.
    /// </summary>
    [JsonProperty("instructions")]
    public string Instructions { get; }

    /// <summary>
    ///     Override the tools the assistant can use for this run.
    ///     This is useful for modifying the behavior on a per-run basis.
    /// </summary>
    [JsonProperty("tools")]
    public IReadOnlyList<Tool> Tools { get; }

    /// <summary>
    ///     Set of 16 key-value pairs that can be attached to an object.
    ///     This can be useful for storing additional information about the object in a structured format.
    ///     Keys can be a maximum of 64 characters long and values can be a maximum of 512 characters long.
    /// </summary>
    [JsonProperty("metadata")]
    public IReadOnlyDictionary<string, string> Metadata { get; }

    [JsonProperty("thread")]
    public CreateThreadRequest ThreadRequest { get; }
}