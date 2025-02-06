using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace LlmTornado.Threads
{
    /// <summary>
    /// Base class representing an annotation in a message with information about
    /// the text to be replaced and the position where it occurs.
    /// </summary>
    [JsonConverter(typeof(MessageAnnotationConverter))]
    public abstract class MessageAnnotation
    {
        /// <summary>
        /// The type of annotation. For example: "file_citation", "file_path", etc.
        /// </summary>
        [JsonProperty("type")]
        public MessageAnnotationType Type { get; set; }

        /// <summary>
        /// The text in the message content that needs to be replaced.
        /// </summary>
        [JsonProperty("text")]
        public string? Text { get; set; }

        /// <summary>
        /// The start index of the text to be replaced.
        /// </summary>
        [JsonProperty("start_index")]
        public int StartIndex { get; private set; }

        /// <summary>
        /// The end index of the text to be replaced.
        /// </summary>
        [JsonProperty("end_index")]
        public int EndIndex { get; private set; }
    }

    /// <summary>
    /// Enumerates the possible types of message annotations, which provide additional
    /// context or metadata about parts of a message, such as references to files or citations.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MessageAnnotationType
    {
        /// <summary>
        /// Represents a message annotation type indicating a file path.
        /// This type is used to annotate messages with information about a specific file path
        /// generated or referenced, such as output files from tools (e.g., code interpreter).
        /// </summary>
        [EnumMember(Value = "file_path")]
        FilePath,

        /// <summary>
        /// Represents an annotation type for a file citation within a message.
        /// This annotation is generated when the assistant references specific content
        /// from a file, typically as a result of a "file_search" operation for retrieving
        /// contextual information or quotes.
        /// </summary>
        [EnumMember(Value = "file_citation")]
        FileCitation
    }

    /// <summary>
    /// A citation within the message that points to a specific quote
    /// from a specific File associated with the assistant or the message.
    /// Generated when the assistant uses the "file_search" tool to search files. 
    /// </summary>
    public sealed class MessageAnnotationFileCitation : MessageAnnotation
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MessageAnnotationFileCitation"/> 
        /// with the type set to "file_citation".
        /// </summary>
        public MessageAnnotationFileCitation()
        {
            Type = MessageAnnotationType.FileCitation;
        }

        /// <summary>
        /// The ID of the specific file the citation is from.
        /// </summary>
        [JsonProperty("file_citation")]
        public FileCitationData? FileCitationData { get; set; }
    }

    /// <summary>
    /// Represents a URL (file path) for the file generated by a tool (for example, code interpreter).
    /// </summary>
    public sealed class MessageAnnotationFilePath : MessageAnnotation
    {
        /// <summary>
        /// Initializes a new instance of <see cref="MessageAnnotationFilePath"/> 
        /// with the type set to "file_path".
        /// </summary>
        public MessageAnnotationFilePath()
        {
            Type = MessageAnnotationType.FilePath;
        }

        /// <summary>
        /// The ID of the file that was generated.
        /// </summary>
        [JsonProperty("file_path")]
        public FilePathData? FilePath { get; set; }
    }


    /// <summary>
    ///     Data related to FilePath annotation
    /// </summary>
    public sealed class FilePathData
    {
        /// <summary>
        ///     The ID of the file that was generated.
        /// </summary>
        [JsonProperty("file_id")]
        public string FileId { get; set; } = null!;
    }

    /// <summary>
    ///     Data related to FileCitation annotation
    /// </summary>
    public sealed class FileCitationData
    {
        /// <summary>
        ///     The ID of the specific File the citation is from.
        /// </summary>
        [JsonProperty("file_id")]
        public string FileId { get; set; } = null!;
    }

    internal class MessageAnnotationConverter : JsonConverter<MessageAnnotation>
    {
        public override void WriteJson(JsonWriter writer, MessageAnnotation? value, JsonSerializer serializer)
        {
            JObject jsonObject = JObject.FromObject(value!, serializer);
            jsonObject.WriteTo(writer);
        }

        public override MessageAnnotation? ReadJson(JsonReader reader, Type objectType,
            MessageAnnotation? existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            JObject jsonObject = JObject.Load(reader);
            string? typeToken = jsonObject["type"]?.ToString();
            if (!Enum.TryParse(typeToken, true, out MessageAnnotationType messageAnnotationType))
            {
                return null;
            }

            return messageAnnotationType switch
            {
                MessageAnnotationType.FileCitation => jsonObject
                    .ToObject<MessageAnnotationFileCitation>(serializer)!,
                MessageAnnotationType.FilePath => jsonObject
                    .ToObject<MessageAnnotationFilePath>(serializer)!,
                _ => null
            };
        }
    }
}