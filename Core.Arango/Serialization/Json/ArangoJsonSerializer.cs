﻿using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Arango.Serialization.Json;

internal class ArangoJsonNullableUnixTimeConverter : JsonConverter<DateTime?>
{
    public override bool HandleNull => true;

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TryGetInt64(out var time))
            return DateTimeOffset.FromUnixTimeMilliseconds(time).UtcDateTime;

        return null;
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteNumberValue(((DateTimeOffset)value.Value).ToUnixTimeMilliseconds());
        else
            writer.WriteNullValue();
    }
}

internal class ArangoJsonNullableUnixTimeDateTimeOffsetConverter : JsonConverter<DateTimeOffset?>
{
    public override bool HandleNull => true;

    public override DateTimeOffset? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TryGetInt64(out var time))
            return DateTimeOffset.FromUnixTimeMilliseconds(time).UtcDateTime;

        return null;
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteNumberValue(value.Value.ToUnixTimeMilliseconds());
        else
            writer.WriteNullValue();
    }
}

internal class ArangoJsonUnixTimeConverter : JsonConverter<DateTime>
{
    public override bool HandleNull => false;

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TryGetInt64(out var time))
            return DateTimeOffset.FromUnixTimeMilliseconds(time).UtcDateTime;

        return DateTime.MinValue;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options) =>
        writer.WriteNumberValue(((DateTimeOffset)value).ToUnixTimeMilliseconds());
}

internal class ArangoJsonUnixTimeDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
    public override bool HandleNull => false;

    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TryGetInt64(out var time))
            return DateTimeOffset.FromUnixTimeMilliseconds(time).UtcDateTime;

        return DateTimeOffset.MinValue;
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options) =>
        writer.WriteNumberValue(value.ToUnixTimeMilliseconds());
}

/// <summary>
///     Arango Json Serializer with System.Json.Text
/// </summary>
public class ArangoJsonSerializer : IArangoSerializer
{
    /// <summary>
    ///  Use unix timestamps for DateTime and DateTimeOffset
    /// </summary>
    public bool UseTimestamps
    {
        get => _useTimestamps;
        set
        {
            _useTimestamps = value;
            if (value && _options.Converters.All(x => x.GetType() != typeof(ArangoJsonUnixTimeConverter)))
            {
                _options.Converters.Add(new ArangoJsonUnixTimeConverter());
                _options.Converters.Add(new ArangoJsonNullableUnixTimeConverter());
                _options.Converters.Add(new ArangoJsonUnixTimeDateTimeOffsetConverter());
                _options.Converters.Add(new ArangoJsonNullableUnixTimeDateTimeOffsetConverter());
            }
        }
    }

    private readonly JsonSerializerOptions _options;
    private bool _useTimestamps;

    /// <summary>
    /// </summary>
    /// <param name="policy">PascalCase or camelCase policy</param>
    public ArangoJsonSerializer(JsonNamingPolicy policy)
    {
        _options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = policy,
            PropertyNameCaseInsensitive = true,
            //DictionaryKeyPolicy = policy,
#if NET6_0_OR_GREATER
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
#else
            IgnoreNullValues = false
#endif
        };
        _options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    }

    /// <inheritdoc />
    public string Serialize(object value) => JsonSerializer.Serialize(value, _options);
    /// <inheritdoc />
    public async Task<byte[]> SerializeAsync(object value, CancellationToken token = default)
    {
        using var ms = new MemoryStream();
        await JsonSerializer.SerializeAsync(ms, value, _options, token).ConfigureAwait(false);
        return ms.ToArray();
    }
    /// <inheritdoc />
    public async ValueTask<T> DeserializeAsync<T>(byte[] buffer, CancellationToken token = default)
    {
        using var ms = new MemoryStream(buffer);
        return await DeserializeAsync<T>(ms, token).ConfigureAwait(false);
    }
    /// <inheritdoc />
    public ValueTask<T> DeserializeAsync<T>(Stream stream, CancellationToken token = default) =>
        JsonSerializer.DeserializeAsync<T>(stream, _options, token);

    /// <inheritdoc />
    public T Deserialize<T>(string value) => JsonSerializer.Deserialize<T>(value, _options);

    /// <inheritdoc />
    public object Deserialize(string value, Type t) => JsonSerializer.Deserialize(value, t, _options);
}