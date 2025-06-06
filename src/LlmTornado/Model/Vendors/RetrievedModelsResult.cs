using System.Collections.Generic;
using LlmTornado.Code;
using LlmTornado.Models.Vendors.Cohere;
using LlmTornado.Models.Vendors.Google;
using Newtonsoft.Json;

namespace LlmTornado.Models.Vendors;

internal class RetrievedModelsResult
{
    [JsonProperty("data")] 
    public List<RetrievedModel> Data { get; set; }

    [JsonProperty("object")] 
    public string? Obj { get; set; }
    
    internal static RetrievedModelsResult? Deserialize(LLmProviders provider, string jsonData, string? postData)
    {
        return provider switch
        {
            LLmProviders.Google => JsonConvert.DeserializeObject<VendorGoogleRetrievedModelsResult>(jsonData)?.ToResult(postData),
            LLmProviders.Cohere => JsonConvert.DeserializeObject<VendorCohereRetrievedModelsResult>(jsonData)?.ToResult(postData),
            _ => JsonConvert.DeserializeObject<RetrievedModelsResult>(jsonData)
        };
    }
}