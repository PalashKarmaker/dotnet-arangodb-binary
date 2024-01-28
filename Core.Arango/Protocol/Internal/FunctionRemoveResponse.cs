using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Core.Arango.Protocol.Internal
{
    internal class FunctionRemoveResponse : ArangoResponseBase
    {
        [JsonPropertyName("deletedCount")]
        [JsonProperty(PropertyName = "deletedCount")]
        public int DeletedCount { get; set; }
    }
}