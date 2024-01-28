using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Core.Arango.Protocol.Internal
{
    internal class TransactionResponse
    {
        [JsonPropertyName("id")]
        [JsonProperty("id")]
        public string Id { get; set; }
    }
}