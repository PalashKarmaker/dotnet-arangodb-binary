using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Core.Arango.Protocol.Internal
{
    internal class EndpointEntity
    {
        [JsonPropertyName("endpoint")]
        [JsonProperty("endpoint")]
        public string Endpoint { get; set; }
    }
}