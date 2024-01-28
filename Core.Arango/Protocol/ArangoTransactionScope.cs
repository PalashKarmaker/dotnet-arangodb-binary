﻿using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Core.Arango.Protocol
{
    /// <summary>
    ///     Arango Transaction Scope
    /// </summary>
    public class ArangoTransactionScope
    {
        /// <summary>
        ///     Collections to read from
        /// </summary>
        [JsonPropertyName("read")]
        [JsonProperty(PropertyName = "read", NullValueHandling = NullValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string> Read { get; set; }

        /// <summary>
        ///     Collections to write to
        /// </summary>
        [JsonPropertyName("write")]
        [JsonProperty(PropertyName = "write", NullValueHandling = NullValueHandling.Ignore)]
        [System.Text.Json.Serialization.JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<string> Write { get; set; }
    }
}