using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace Core.Arango.Protocol.Internal
{
    internal class SingleResult<T>
    {
        [JsonPropertyName("result")]
        [JsonProperty(PropertyName = "result")]
        public T Result { get; set; }
    }
}