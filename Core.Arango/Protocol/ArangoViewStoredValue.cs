using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Core.Arango.Protocol
{
    /// <summary>
    /// </summary>
    public class ArangoViewStoredValue
    {
        /// <summary>
        ///     Attribute paths to store
        /// </summary>
        [JsonPropertyName("fields")]
        [JsonProperty(PropertyName = "fields")]
        public List<string> Fields { get; set; }

        /// <summary>
        ///     Compression
        /// </summary>
        [JsonPropertyName("compression")]
        [JsonProperty(PropertyName = "compression")]
        public ArangoViewCompressionType Compression { get; set; }
    }
}