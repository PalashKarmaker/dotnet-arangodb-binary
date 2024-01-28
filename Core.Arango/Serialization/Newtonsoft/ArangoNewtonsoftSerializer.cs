using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Arango.Serialization.Newtonsoft;

/// <summary>
///     Arango Json Serializer with Newtonsoft
/// </summary>
public class ArangoNewtonsoftSerializer : IArangoSerializer
{
    private readonly JsonSerializerSettings _settings;

    /// <summary>
    ///     Arango Json Serializer with Newtonsoft
    /// </summary>
    /// <param name="resolver">PascalCase or camelCaseResolver</param>
    public ArangoNewtonsoftSerializer(IContractResolver resolver) =>
        _settings = new JsonSerializerSettings
        {
            ContractResolver = resolver,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Formatting = Formatting.None,
            DateFormatHandling = DateFormatHandling.IsoDateFormat,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc
        };

    /// <inheritdoc />
    public string Serialize(object value) => JsonConvert.SerializeObject(value, _settings);

    /// <inheritdoc />
    public T Deserialize<T>(string value) => JsonConvert.DeserializeObject<T>(value, _settings);

    /// <inheritdoc />
    public object Deserialize(string v, Type t) => JsonConvert.DeserializeObject(v, t, _settings);
    /// <inheritdoc />
    public Task<byte[]> SerializeAsync(object value, CancellationToken token = default)
    {
        return Task.Run(() =>
        {
            var str = Serialize(value);
            return Encoding.UTF8.GetBytes(str);
        });
    }
    /// <inheritdoc />
    public async ValueTask<T> DeserializeAsync<T>(byte[] buffer, CancellationToken token = default)
    {
        var obj = await Task.Run(() =>
        {
            var str = Encoding.UTF8.GetString(buffer);
            return Deserialize<T>(str);
        }).ConfigureAwait(false);
        return await ValueTask.FromResult(obj);
    }
    /// <inheritdoc />
    public async ValueTask<T> DeserializeAsync<T>(Stream stream, CancellationToken token = default)
    {
        var obj = await Task.Run(() =>
        {
            using var reader = new StreamReader(stream);
            JsonSerializer jsonSerializer = new();
            var res = jsonSerializer.Deserialize(reader, typeof(T));
            return (T)res;
        }).ConfigureAwait(false);
        return obj;
    }
}