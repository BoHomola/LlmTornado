﻿using Newtonsoft.Json;
using LlmTornado.Code;

namespace LlmTornado.Demo;

    
public enum Demos
{
    Unknown,
    [Flaky("Deprecated by OpenAI")]
    ChatVision,
    [Flaky("Deprecated by OpenAI")]
    ChatVisionBase64,
    AssistantCreate,
    AssistantList,
    AssistantRetrieve,
    AssistantModify,
    AssistantDelete,
    [Flaky]
    AssistantDeleteAllDemoAssistants,
    [Flaky]
    FilesUpload,
    [Flaky]
    ImagesGenerate,
    [Flaky]
    ThreadsCreate,
    [Flaky]
    ThreadsRetrieve,
    [Flaky]
    ThreadsModify,
    [Flaky]
    ThreadsDelete,
    VectorStoreCreate,
    VectorStoreRetrieve,
    VectorStoreList,
    VectorStoreModify,
    VectorStoreFilesCreate,
    VectorStoreFilesCreateCustomChunkingStraegy,
    VectorStoreFilesList,
    VectorStoreFilesRetrieve,
    VectorStoreFilesDelete,
    VectorStoreFileBatchCreate,
    VectorStoreFileBatchRetrieve,
    VectorStoreBatchFileList,
    VectorStoreFileBatchCancel,
    VectorStoreDelete,
    [Flaky("only assistants v1 are supported")]
    ThreadsCreateMessage,
    ChatCompletion,
    ChatStreamWithFunctions,
    ChatAnthropic,
    ChatStreamingAnthropic,
    ChatAzure,
    ChatOpenAiFunctions,
    ChatAnthropicFunctions,
    ChatAnthropicFailFunctions,
    ChatCohere,
    ChatCohereStreaming,
    [Flaky("covered by other tests, takes a long time to finish ")]
    ChatAllVendors,
    Embedding,
    ChatFunctionRequired,
    ChatCohereWebSearch,
    ChatCohereWebSearchStreaming,
    [Flaky("interactive demo")]
    OpenAiFunctionsStreamingInteractive,
    ChatAnthropicParallelFunctions,
    [Flaky("interactive demo")]
    AnthropicFunctionsStreamingInteractive,
    [Flaky("interactive demo")]
    CohereFunctionsStreamingInteractive,
    [Flaky("interactive demo")]
    CrossVendorFunctionsStreamingInteractive,
    DisableParallelTools,
    ChatGoogle,
    ChatGoogleFunctions,
    ChatGoogleStream,
    EmbeddingOpenAiScalar,
    EmbeddingOpenAiVector,
    EmbeddingCohereScalar,
    EmbeddingCohereVector,
    EmbeddingCohereScalarExtensions,
    Chat4OMini,
    ChatGroq,
    ChatGroqStreaming,
    Chat4OStructuredJson,
    ChatCohere2408,
    ChatOpenAiO1,
    ChatHaiku35,
    ChatAudioWav,
    ChatAudioMp3,
    ChatAudioInAudioOut,
    ChatAudioMultiturn,
    TranscriptionWhisperV2Text,
    TranscriptionWhisperV2Json,
    TranscriptionWhisperV2Srt,
    TranscriptionWhisperV2JsonVerbose,
    TranscriptionWhisperV3TurboJsonVerbose,
    ChatAudioWavStreaming,
    ChatAudioInAudioOutWavStreaming,
    ChatToolsGemini,
    ChatToolsGeminiStrict,
    ChatCompletionO1Developer,
    [Flaky("requires ollama")]
    CustomProviderOllama,
    [Flaky("requires ollama")]
    CustomProviderOllamaStreaming,
    ChatAnthropicCaching,
    [Flaky("interactive")]
    ChatAnthropicCachingInteractive,
    Last
}

public class Program
{
    private static Demos selectedDemo = Demos.Unknown;
    public static Keys ApiKeys { get; set; }

    public class AzureKey
    {
        public string Version { get; set; }
        public string ApiUrlFormat { get; set; }
        public string Key { get; set; }
    }
    
    public class Keys
    {
        public string OpenAi { get; set; }
        public string Anthropic { get; set; }
        public string Cohere { get; set; }
        public string Google { get; set; }
        public string Groq { get; set; }
        public AzureKey Azure { get; set; }
    }

    public static TornadoApi ConnectMulti(bool httpStrict = true)
    {
        TornadoApi tornadoApi = new TornadoApi([
            new ProviderAuthentication(LLmProviders.OpenAi, ApiKeys.OpenAi),
            new ProviderAuthentication(LLmProviders.Anthropic, ApiKeys.Anthropic),
            new ProviderAuthentication(LLmProviders.Cohere, ApiKeys.Cohere),
            new ProviderAuthentication(LLmProviders.Google, ApiKeys.Google),
            new ProviderAuthentication(LLmProviders.Groq, ApiKeys.Groq)
        ])
        {
            httpStrict = httpStrict
        };

        return tornadoApi;
    }
    
    public static TornadoApi Connect(LLmProviders provider = LLmProviders.OpenAi)
    {
        return ConnectMulti();
    }

    public static async Task<bool> SetupApi()
    {
        string? projectDirectory = Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName;

        if (string.IsNullOrWhiteSpace(projectDirectory))
        {
            Console.WriteLine("Failed to read project directory path, see Program.cs, SetupApi()");
            return false;
        }

        string apiKeyFileLocation = Path.Join([projectDirectory, "apiKey.json"]);
        if (!File.Exists(apiKeyFileLocation))
        {
            Console.WriteLine("Please copy and paste apiKeyPrototype.json file in the same folder, rename the copy as apiKey.json and replace the string inside with your API key");
            return false;
        }

        string apiKey = await File.ReadAllTextAsync(apiKeyFileLocation);

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            Console.WriteLine("API key not set, please place your API key in apiKey.json file");
            return false;
        }

        ApiKeys = JsonConvert.DeserializeObject<Keys>(apiKey) ?? throw new Exception("Invalid content of apiKey.json");
        return true;
    }

    public static Func<Task>? GetDemo(Demos demo)
    {
        Func<Task>? task = demo switch
        {
            Demos.ChatVisionBase64 => VisionDemo.VisionBase64,
            Demos.ChatVision => VisionDemo.VisionBase64,
            Demos.AssistantList => AssistantsDemo.List,
            Demos.AssistantCreate => AssistantsDemo.Create,
            Demos.AssistantRetrieve => AssistantsDemo.Retrieve,
            Demos.AssistantModify => AssistantsDemo.Modify,
            Demos.AssistantDelete => AssistantsDemo.Delete,
            Demos.AssistantDeleteAllDemoAssistants => AssistantsDemo.DeleteAllDemoAssistants,
            Demos.VectorStoreCreate => VectorStoreDemo.CreateVectorStore,
            Demos.VectorStoreRetrieve => VectorStoreDemo.RetrieveVectorStore,
            Demos.VectorStoreList => VectorStoreDemo.ListVectorStores,
            Demos.VectorStoreModify => VectorStoreDemo.ModifyVectorStore,
            Demos.VectorStoreFilesCreate => VectorStoreDemo.CreateVectorStoreFile,
            Demos.VectorStoreFilesCreateCustomChunkingStraegy => VectorStoreDemo.CreateVectorStoreFileCustomChunkingStrategy,
            Demos.VectorStoreFilesList => VectorStoreDemo.ListVectorStoreFiles,
            Demos.VectorStoreFilesRetrieve => VectorStoreDemo.RetrieveVectorStoreFile,
            Demos.VectorStoreFilesDelete => VectorStoreDemo.DeleteVectorStoreFile,
            Demos.VectorStoreFileBatchCreate => VectorStoreDemo.CreateVectorStoreFileBatch,
            Demos.VectorStoreBatchFileList => VectorStoreDemo.ListVectorStoreBatchFiles,
            Demos.VectorStoreFileBatchRetrieve => VectorStoreDemo.RetrieveVectorStoreFileBatch,
            Demos.VectorStoreFileBatchCancel => VectorStoreDemo.CancelVectorStoreFileBatch,
            Demos.VectorStoreDelete => VectorStoreDemo.DeleteVectorStore,
            Demos.FilesUpload => FilesDemo.Upload,
            Demos.ImagesGenerate => ImagesDemo.Generate,
            Demos.ThreadsCreate => ThreadsDemo.Create,
            Demos.ThreadsRetrieve => ThreadsDemo.Retrieve,
            Demos.ThreadsModify => ThreadsDemo.Modify,
            Demos.ThreadsDelete => ThreadsDemo.Delete,
            Demos.ThreadsCreateMessage => ThreadsDemo.CreateMessage,
            Demos.ChatCompletion => ChatDemo.Completion,
            Demos.ChatStreamWithFunctions => ChatDemo.StreamWithFunctions,
            Demos.ChatAnthropic => ChatDemo.Anthropic,
            Demos.ChatStreamingAnthropic => ChatDemo.AnthropicStreaming,
            Demos.ChatAzure => ChatDemo.Azure,
            Demos.ChatOpenAiFunctions => ChatDemo.OpenAiFunctions,
            Demos.ChatAnthropicFunctions => ChatDemo.AnthropicStreamingFunctions,
            Demos.ChatAnthropicFailFunctions => ChatDemo.AnthropicFailFunctions,
            Demos.ChatCohere => ChatDemo.Cohere,
            Demos.ChatCohereStreaming => ChatDemo.CohereStreaming,
            Demos.ChatAllVendors => ChatDemo.AllChatVendors,
            Demos.Embedding => EmbeddingDemo.Embed,
            Demos.ChatFunctionRequired => ChatDemo.ChatFunctionRequired,
            Demos.ChatCohereWebSearch => ChatDemo.CohereWebSearch,
            Demos.ChatCohereWebSearchStreaming => ChatDemo.CohereWebSearchStreaming,
            Demos.OpenAiFunctionsStreamingInteractive => ChatDemo.OpenAiFunctionsStreamingInteractive,
            Demos.ChatAnthropicParallelFunctions => ChatDemo.AnthropicFunctionsParallel,
            Demos.AnthropicFunctionsStreamingInteractive => ChatDemo.AnthropicFunctionsStreamingInteractive,
            Demos.CohereFunctionsStreamingInteractive => ChatDemo.CohereFunctionsStreamingInteractive,
            Demos.CrossVendorFunctionsStreamingInteractive => ChatDemo.CrossVendorFunctionsStreamingInteractive,
            Demos.DisableParallelTools => ChatDemo.OpenAiDisableParallelFunctions,
            Demos.ChatGoogle => ChatDemo.Google,
            Demos.ChatGoogleFunctions => ChatDemo.GoogleFunctions,
            Demos.ChatGoogleStream => ChatDemo.GoogleStream,
            Demos.EmbeddingOpenAiScalar => EmbeddingDemo.Embed,
            Demos.EmbeddingOpenAiVector => EmbeddingDemo.EmbedVector,
            Demos.EmbeddingCohereScalar => EmbeddingDemo.EmbedCohere,
            Demos.EmbeddingCohereVector => EmbeddingDemo.EmbedCohereVector,
            Demos.EmbeddingCohereScalarExtensions => EmbeddingDemo.EmbedCohereExtensions,
            Demos.Chat4OMini => ChatDemo.Completion4Mini,
            Demos.ChatGroq => ChatDemo.CompletionGroq,
            Demos.ChatGroqStreaming => ChatDemo.GroqStreaming,
            Demos.Chat4OStructuredJson => ChatDemo.Completion4OStructuredJson,
            Demos.ChatCohere2408 => ChatDemo.Cohere2408,
            Demos.ChatOpenAiO1 => ChatDemo.OpenAiO1,
            Demos.ChatHaiku35 => ChatDemo.Haiku35,
            Demos.ChatAudioWav => ChatDemo.AudioInWav,
            Demos.ChatAudioMp3 => ChatDemo.AudioInMp3,
            Demos.ChatAudioInAudioOut => ChatDemo.AudioInAudioOutWav,
            Demos.ChatAudioMultiturn => ChatDemo.AudioInAudioOutMultiturn,
            Demos.TranscriptionWhisperV2Text => TranscriptionDemo.TranscribeFormatText,
            Demos.TranscriptionWhisperV2Json => TranscriptionDemo.TranscribeFormatJson,
            Demos.TranscriptionWhisperV2Srt => TranscriptionDemo.TranscribeFormatSrt,
            Demos.TranscriptionWhisperV2JsonVerbose => TranscriptionDemo.TranscribeFormatJsonVerbose,
            Demos.TranscriptionWhisperV3TurboJsonVerbose => TranscriptionDemo.TranscribeFormatJsonVerboseGroq,
            Demos.ChatAudioWavStreaming => ChatDemo.AudioInWavStreaming,
            Demos.ChatAudioInAudioOutWavStreaming => ChatDemo.AudioInAudioOutWavStreaming,
            Demos.ChatToolsGemini => ChatDemo.ChatFunctionGemini,
            Demos.ChatToolsGeminiStrict => ChatDemo.ChatFunctionGeminiStrict,
            Demos.ChatCompletionO1Developer => ChatDemo.CompletionO1Developer,
            Demos.CustomProviderOllama => CustomProviderDemo.Ollama,
            Demos.CustomProviderOllamaStreaming => CustomProviderDemo.OllamaStreaming,
            Demos.ChatAnthropicCaching => ChatDemo.AnthropicCaching,
            Demos.ChatAnthropicCachingInteractive => ChatDemo.AnthropicCachingChat,
            _ => null
        };

        return task;
    }
    
    public static async Task Main(string[] args)
    {
        Console.Title = "LlmTornado Demo";

        if (!await SetupApi())
        {
            return;
        }

        Demos? forceDemo = null;
        
        selectedDemo = forceDemo ?? Demos.Last - 1;
        Func<Task>? task = GetDemo(selectedDemo);

        if (task is not null)
        {
            await task.Invoke();
        }

        Console.ReadKey();
    }
}