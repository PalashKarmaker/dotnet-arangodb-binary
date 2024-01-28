using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Core.Arango.Protocol.Internal
{
    internal class EndpointResponse
    {
        [JsonPropertyName("endpoints")]
        [JsonProperty("endpoints")]
        public List<EndpointEntity> Endpoints { get; set; }
    }
}