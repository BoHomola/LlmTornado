﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LlmTornado.Images;
using LlmTornado;
using LlmTornado.ChatFunctions;
using LlmTornado.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace LlmTornado.Code;

public class Ref<T>
{
    public T? Ptr { get; set; }
}

internal class StreamResponse
{
    public Stream Stream { get; set; }
    public ApiResultBase Headers { get; set; }
    public HttpResponseMessage Response { get; set; }
}

/// <summary>
///     A failed HTTP request.
/// </summary>
public class HttpFailedRequest
{
    /// <summary>
    ///     The exception with details what went wrong.
    /// </summary>
    public Exception Exception { get; set; }
    
    /// <summary>
    ///     The request that failed.
    /// </summary>
    public HttpCallRequest? Request { get; set; }
    
    /// <summary>
    ///     Result of the failed request.
    /// </summary>
    public IHttpCallResult? Result { get; set; }
    
    /// <summary>
    ///     Raw message of the failed request. Do not dispose this, it will be disposed automatically by Tornado.
    /// </summary>
    public HttpResponseMessage RawMessage { get; set; }
    
    /// <summary>
    ///     Body of the request.
    /// </summary>
    public TornadoRequestContent Body { get; set; }
}

/// <summary>
///     Streaming HTTP request.
/// </summary>
public class TornadoStreamRequest : IAsyncDisposable
{
    public Stream? Stream { get; set; }
    public HttpResponseMessage? Response { get; set; }
    public StreamReader? StreamReader { get; set; }
    public Exception? Exception { get; set; }
    public HttpCallRequest? CallRequest { get; set; }
    public IHttpCallResult? CallResponse { get; set; }

    /// <summary>
    ///     Disposes the underlying stream.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (Stream is not null)
        {
            await Stream.DisposeAsync().ConfigureAwait(false);   
        }

        Response?.Dispose();
        StreamReader?.Dispose();
        GC.SuppressFinalize(this);
    }
}

/// <summary>
///     Roles of chat participants.
/// </summary>
public enum ChatMessageRoles
{
    /// <summary>
    ///     Unknown role.
    /// </summary>
    Unknown,
    /// <summary>
    ///     System prompt / preamble / developer message.
    /// </summary>
    System,
    /// <summary>
    ///     Messages written by user.
    /// </summary>
    User,
    /// <summary>
    ///     Assistant messages.
    /// </summary>
    Assistant,
    /// <summary>
    ///     Messages representing tool/function/connector usage.
    /// </summary>
    Tool
}

/// <summary>
///     Level of reasoning suggested.
/// </summary>
public enum ChatReasoningEfforts
{
    /// <summary>
    ///     Low reasoning - fast responses
    /// </summary>
    [JsonProperty("low")]
    Low,
    /// <summary>
    ///     Balanced reasoning
    /// </summary>
    [JsonProperty("medium")]
    Medium,
    /// <summary>
    ///     High reasoning - slow responses
    /// </summary>
    [JsonProperty("high")]
    High
}

internal enum ChatResultStreamInternalKinds
{
    Unknown,
    None,
    AppendAssistantMessage
}

public class ChatFunctionParamsGetter
{
    internal Dictionary<string, object?>? Source { get; set; }

    public ChatFunctionParamsGetter(Dictionary<string, object?>? pars)
    {
        Source = pars;
    }
}

internal class ToolCallInboundAccumulator
{
    public ToolCall ToolCall { get; set; }
    public StringBuilder ArgumentsBuilder { get; set; } = new StringBuilder();
}

/// <summary>
///     Audio block content.
/// </summary>
public class ChatMessageAudio
{
    /// <summary>
    ///     Unique identifier for this audio response.
    /// </summary>
    [JsonProperty("id")]
    public string Id { get; set; }
    
    /// <summary>
    ///     The Unix timestamp (in seconds) for when this audio response will no longer be accessible on the server for use in multi-turn conversations.
    /// </summary>
    [JsonProperty("expires_at")]
    public long ExpiresAt { get; set; }
    
    /// <summary>
    ///     Base64 encoded audio bytes generated by the model, in the format specified in the request.
    /// </summary>
    [JsonProperty("data")]
    public string? Data { get; set; }

    /// <summary>
    ///     Converts <see cref="Data"/> from base64 to a byte array.
    /// </summary>
    public byte[] ByteData => Data is null ? [] : Convert.FromBase64String(Data);
    
    /// <summary>
    ///     Transcript of the audio generated by the model.
    /// </summary>
    [JsonProperty("transcript")]
    public string Transcript { get; set; }
}

/// <summary>
///     Strategies for reducing audio modality pricing.
/// </summary>
public enum ChatAudioCompressionStrategies
{
    /// <summary>
    ///     Audio encoding is preferred both for input and output.
    /// </summary>
    Native,
    
    /// <summary>
    ///     Output is encoded as text when possible.
    /// </summary>
    OutputAsText,
    
    /// <summary>
    ///     Output is encoded as previous audio id when not expired; falls to <see cref="OutputAsText"/> otherwise.
    /// </summary>
    PreferNative
}

/// <summary>
///     Audio settings.
/// </summary>
public class ChatRequestAudio
{
    /// <summary>
    ///     The voice to use.
    /// </summary>
    [JsonProperty("voice")]
    [JsonConverter(typeof(StringEnumConverter), true)]
    public ChatAudioRequestKnownVoices Voice { get; set; }
    
    /// <summary>
    ///     The output audio format.
    /// </summary>
    [JsonProperty("format")]
    [JsonConverter(typeof(StringEnumConverter), true)]
    public ChatRequestAudioFormats Format { get; set; }

    /// <summary>
    ///     The compression strategy to use when serializing requests.
    /// </summary>
    [JsonIgnore] 
    public ChatAudioCompressionStrategies CompressionStrategy { get; set; } = ChatAudioCompressionStrategies.PreferNative;
    
    /// <summary>
    ///     Creates a new audio settings from a known voice.
    /// </summary>
    /// <param name="voice"></param>
    /// <param name="format"></param>
    public ChatRequestAudio(ChatAudioRequestKnownVoices voice, ChatRequestAudioFormats format)
    {
        Voice = voice;
        Format = format;
    }
}

/// <summary>
///     Formats in which the transcription can be returned.
/// </summary>
public enum AudioTranscriptionResponseFormats
{
    /// <summary>
    ///     JSON.
    /// </summary>
    [JsonProperty("json")]
    Json,
    
    /// <summary>
    ///     Plaintext.
    /// </summary>
    [JsonProperty("text")]
    Text,
    
    /// <summary>
    ///     SubRip Subtitle.
    /// </summary>
    [JsonProperty("srt")]
    Srt,
    
    /// <summary>
    ///     Json with details.
    /// </summary>
    [JsonProperty("verbose_json")]
    VerboseJson,
    
    /// <summary>
    ///     Video Text to Track.
    /// </summary>
    [JsonProperty("vtt")]
    Vtt
}

/// <summary>
///     Output audio formats.
/// </summary>
public enum ChatRequestAudioFormats
{
    /// <summary>
    ///     Waveform
    /// </summary>
    [JsonProperty("wav")]
    Wav,
    /// <summary>
    ///     MP3
    /// </summary>
    [JsonProperty("mp3")]
    Mp3,
    /// <summary>
    ///     Flac
    /// </summary>
    [JsonProperty("flac")]
    Flac,
    /// <summary>
    ///     Opus
    /// </summary>
    [JsonProperty("opus")]
    Opus,
    /// <summary>
    ///    Pulse-code modulation. Supported in streaming mode.
    /// </summary>
    [JsonProperty("pcm16")]
    Pcm16
}

/// <summary>
///     Known chat request audio settings voices.
/// </summary>
public enum ChatAudioRequestKnownVoices
{
    /// <summary>
    ///     Male voice, deep.
    /// </summary>
    [JsonProperty("ash")]
    
    Ash,
    /// <summary>
    ///     Male voice, younger.
    /// </summary>
    [JsonProperty("ballad")]
    Ballad,
    
    [JsonProperty("coral")]
    Coral,
    [JsonProperty("sage")]
    Sage,
    [JsonProperty("verse")]
    Verse,
    
    /// <summary>
    ///     Not recommended by OpenAI, less expressive.
    /// </summary>
    [JsonProperty("alloy")]
    Alloy,
    /// <summary>
    ///     Not recommended by OpenAI, less expressive.
    /// </summary>
    [JsonProperty("echo")]
    Echo,
    /// <summary>
    ///     Not recommended by OpenAI, less expressive.
    /// </summary>
    [JsonProperty("summer")]
    Summer
}

/// <summary>
///     Represents modalities of chat models.
/// </summary>
public enum ChatModelModalities
{
    /// <summary>
    ///     Model is capable of generating text.
    /// </summary>
    Text,
    /// <summary>
    ///     Model is capable of generating audio.
    /// </summary>
    Audio
}

/// <summary>
///     Represents an audio part of a chat message.
/// </summary>
public class ChatAudio
{
    /// <summary>
    ///     Base64 encoded audio data.
    /// </summary>
    public string Data { get; set; }
    
    /// <summary>
    ///     Format of the encoded audio data.
    /// </summary>
    public ChatAudioFormats Format { get; set; }

    /// <summary>
    ///     Creates an empty audio instance.
    /// </summary>
    public ChatAudio()
    {
        
    }

    /// <summary>
    ///     Creates an audio instance from data and format.
    /// </summary>
    /// <param name="data">Base64 encoded audio data</param>
    /// <param name="format">Format of the audio</param>
    public ChatAudio(string data, ChatAudioFormats format)
    {
        Data = data;
        Format = format;
    }
}

/// <summary>
///     Supported audio formats.
/// </summary>
public enum ChatAudioFormats
{
    /// <summary>
    /// Wavelet
    /// </summary>
    Wav,
    /// <summary>
    /// MP3
    /// </summary>
    Mp3
}

/// <summary>
///     Represents a chat image
/// </summary>
public class ChatImage
{
    /// <summary>
    ///     Creates a new chat image
    /// </summary>
    /// <param name="content">Publicly available URL to the image or base64 encoded content</param>
    public ChatImage(string content)
    {
        Url = content;
    }

    /// <summary>
    ///     Creates a new chat image
    /// </summary>
    /// <param name="content">Publicly available URL to the image or base64 encoded content</param>
    /// <param name="detail">The detail level to use, defaults to <see cref="ImageDetail.Auto" /></param>
    public ChatImage(string content, ImageDetail? detail)
    {
        Url = content;
        Detail = detail;
    }
    
    /// <summary>
    ///     When using base64 encoding in <see cref="Url"/>, certain providers such as Google require <see cref="MimeType"/> to be set.
    ///     Values supported by Google: image/png, image/jpeg
    /// </summary>
    [JsonIgnore]
    public string? MimeType { get; set; }

    /// <summary>
    ///     Publicly available URL to the image or base64 encoded content
    /// </summary>
    [JsonProperty("url")]
    public string Url { get; set; }

    /// <summary>
    ///     Publicly available URL to the image or base64 encoded content
    /// </summary>
    [JsonProperty("detail")]
    [JsonConverter(typeof(StringEnumConverter))]
    public ImageDetail? Detail { get; set; }
}

/// <summary>
/// Known LLM providers.
/// </summary>
public enum LLmProviders
{
    /// <summary>
    /// Provider not resolved.
    /// </summary>
    Unknown,
    /// <summary>
    /// OpenAI.
    /// </summary>
    OpenAi,
    /// <summary>
    /// Anthropic.
    /// </summary>
    Anthropic,
    /// <summary>
    /// Azure OpenAI.
    /// </summary>
    AzureOpenAi,
    /// <summary>
    /// Cohere.
    /// </summary>
    Cohere,
    /// <summary>
    /// KoboldCpp, Ollama and other self-hosted providers.
    /// </summary>
    Custom,
    /// <summary>
    /// Google.
    /// </summary>
    Google,
    /// <summary>
    /// Groq.
    /// </summary>
    Groq,
    /// <summary>
    /// Internal value.
    /// </summary>
    Length
}

/// <summary>
/// 
/// </summary>
public enum CapabilityEndpoints
{
    Chat,
    Moderation,
    Completions,
    Embeddings,
    Models,
    Files,
    ImageGeneration,
    Audio,
    Assistants,
    ImageEdit,
    Threads,
    FineTuning
}

/// <summary>
/// Represents authentication to a single provider.
/// </summary>
public class ProviderAuthentication
{
    public LLmProviders Provider { get; set; }
    public string? ApiKey { get; set; }
    public string? Organization { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="providers"></param>
    /// <param name="ApiKey"></param>
    /// <param name="organization"></param>
    public ProviderAuthentication(LLmProviders provider, string apiKey, string? organization = null)
    {
        Provider = provider;
        ApiKey = apiKey;
        Organization = organization;
    }
}

/// <summary>
/// Types of inbound streams.
/// </summary>
public enum StreamRequestTypes
{
    /// <summary>
    /// Unrecognized stream.
    /// </summary>
    Unknown,
    /// <summary>
    /// Chat/completion stream.
    /// </summary>
    Chat
}

/// <summary>
///  A Tornado HTTP request.
/// </summary>
public class TornadoRequestContent
{
    /// <summary>
    /// Content of the request.
    /// </summary>
    public string Body { get; set; }
    
    /// <summary>
    /// Forces the URl to differ from the one inferred further down the pipeline.
    /// </summary>
    public string? Url { get; set; }

    internal TornadoRequestContent(string body, string? url = null)
    {
        Body = body;
        Url = url;
    }

    internal TornadoRequestContent()
    {
        
    }
}