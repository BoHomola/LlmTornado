﻿using System;
using System.Collections.Generic;
using LlmTornado.Audio.Models;
using LlmTornado.Chat;
using LlmTornado.Chat.Models;
using LlmTornado.Chat.Vendors.Anthropic;
using LlmTornado.Chat.Vendors.Cohere;
using LlmTornado.Code;
using Newtonsoft.Json;

namespace LlmTornado.Audio;

/// <summary>
///     Transcribes audio into the input language.
/// </summary>
public class TranscriptionRequest
{
    /// <summary>
    ///     The language of the input audio. Supplying the input language in ISO-639-1 format will improve accuracy and
    ///     latency.Visit to list ISO-639-1 formats <see href="https://en.wikipedia.org/wiki/List_of_ISO_639-1_codes" />
    /// </summary>
    public string Language { get; set; }
    
    /// <summary>
    ///     The audio file to transcribe, in one of these formats: mp3, mp4, mpeg, mpga, m4a, wav, or webm
    /// </summary>
    public AudioFile File { get; set; }

    /// <summary>
    ///     ID of the model to use. Only whisper-1 is currently available.
    /// </summary>
    public AudioModel Model { get; set; } = AudioModel.OpenAi.Whisper.V2;

    /// <summary>
    ///     An optional text to guide the model's style or continue a previous audio segment. The Prompt should match the audio
    ///     language. Please review href="https://platform.openai.com/docs/guides/speech-to-text/prompting"/>
    /// </summary>
    public string Prompt { get; set; }

    /// <summary>
    ///     The format of the transcript output, in one of these options: json, text, srt, verbose_json, or vtt.
    /// </summary>
    public AudioTranscriptionResponseFormats ResponseFormat { get; set; } = AudioTranscriptionResponseFormats.VerboseJson;

    internal string GetResponseFormat => ResponseFormat switch
    {
        AudioTranscriptionResponseFormats.Json => "json",
        AudioTranscriptionResponseFormats.Text => "text",
        AudioTranscriptionResponseFormats.Srt => "srt",
        AudioTranscriptionResponseFormats.VerboseJson => "verbose_json",
        AudioTranscriptionResponseFormats.Vtt => "vtt",
        _ => string.Empty
    };
    
    /// <summary>
    ///     The sampling temperature, between 0 and 1. Higher values like 0.8 will make the output more random, while lower
    ///     values like 0.2 will make it more focused and deterministic. If set to 0, the model will use log probability to
    ///     automatically increase the temperature until certain thresholds are hit.
    /// </summary>
    public float? Temperature { get; set; }
    
    [JsonIgnore]
    internal string? UrlOverride { get; set; }
    
    private static readonly Dictionary<LLmProviders, Func<TranscriptionRequest, IEndpointProvider, string>> SerializeMap = new Dictionary<LLmProviders, Func<TranscriptionRequest, IEndpointProvider, string>>
    {
        { LLmProviders.OpenAi, (x, y) => JsonConvert.SerializeObject(x, EndpointBase.NullSettings) },
        { LLmProviders.Groq, (x, y) => JsonConvert.SerializeObject(x, EndpointBase.NullSettings) }
    };
    
    /// <summary>
    ///		Serializes the chat request into the request body, based on the conventions used by the LLM provider.
    /// </summary>
    /// <param name="provider"></param>
    /// <returns></returns>
    public TornadoRequestContent Serialize(IEndpointProvider provider)
    {
        return SerializeMap.TryGetValue(provider.Provider, out Func<TranscriptionRequest, IEndpointProvider, string>? serializerFn) ? new TornadoRequestContent(serializerFn.Invoke(this, provider), UrlOverride) : new TornadoRequestContent(string.Empty, UrlOverride);
    }
}