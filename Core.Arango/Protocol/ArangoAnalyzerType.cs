﻿using Core.Arango.Serialization.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Core.Arango.Protocol
{
    /// <summary>
    ///     AnalyzerType
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
    public enum ArangoAnalyzerType
    {
        /// <summary>
        ///     Treat value as atom (no transformation)
        /// </summary>
        [EnumMember(Value = "identity")] Identity,

        /// <summary>
        ///     Split into tokens at user-defined character
        /// </summary>
        [EnumMember(Value = "delimiter")] Delimiter,

        /// <summary>
        ///     Apply stemming to the value as a whole
        /// </summary>
        [EnumMember(Value = "stem")] Stem,

        /// <summary>
        ///     Apply normalization to the value as a whole
        /// </summary>
        [EnumMember(Value = "norm")] Norm,

        /// <summary>
        ///     Create n-grams from value with user-defined lengths
        /// </summary>
        [EnumMember(Value = "ngram")] Ngram,

        /// <summary>
        ///     Tokenize into words, optionally with stemming, normalization, stop-word filtering and edge n-gram generation
        /// </summary>
        [EnumMember(Value = "text")] Text,

        /// <summary>
        ///    An Analyzer capable of converting the input into a set of language-specific tokens. 
        ///    This makes comparisons follow the rules of the respective language, most notable in range queries against Views.
        /// </summary>
        [EnumMember(Value = "collation")] Collation,

        /// <summary>
        ///     An Analyzer capable of running a restricted AQL query to perform data manipulation / filtering.
        /// </summary>
        [EnumMember(Value = "aql")] Aql,

        /// <summary>
        ///     An Analyzer capable of chaining effects of multiple Analyzers into one.
        ///     The pipeline is a list of Analyzers, where the output of an Analyzer is passed to the next for further processing.
        ///     The final token value is determined by last Analyzer in the pipeline.
        /// </summary>
        [EnumMember(Value = "pipeline")] Pipeline,

        /// <summary>
        ///     An Analyzer capable of removing specified tokens from the input.
        /// </summary>
        [EnumMember(Value = "stopwords")] Stopwords,

        /// <summary>
        ///     An Analyzer capable of breaking up the input text into tokens in a language-agnostic manner as per Unicode Standard Annex #29, making it suitable for mixed language strings. 
        ///     It can optionally preserve all non-whitespace or all characters instead of keeping alphanumeric characters only, as well as apply case conversion.
        /// </summary>
        [EnumMember(Value = "segmentation")] Segmentation,

        /// <summary>
        ///     An Analyzer capable of breaking up a GeoJSON object into a set of indexable tokens for further usage with
        ///     ArangoSearch Geo functions.
        /// </summary>
        [EnumMember(Value = "geojson")] GeoJson,

        /// <summary>
        ///     An Analyzer capable of breaking up JSON object describing a coordinate into a set of indexable tokens for further
        ///     usage with ArangoSearch Geo functions.
        /// </summary>
        [EnumMember(Value = "geopoint")] GeoPoint
    }
}