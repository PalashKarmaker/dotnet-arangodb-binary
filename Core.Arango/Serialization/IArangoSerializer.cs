using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Arango.Serialization
{
    /// <summary>
    ///     Arango Serializer Interface
    /// </summary>
    public interface IArangoSerializer
    {
        /// <summary>
        ///     Convert object to string
        /// </summary>
        public string Serialize(object value);

        /// <summary>
        ///     Convert string to object
        /// </summary>
        public T Deserialize<T>(string value);

        /// <summary>
        ///     Convert string to object
        /// </summary>
        public object Deserialize(string value, Type type);
        Task<byte[]> SerializeAsync(object value, CancellationToken token = default);
        ValueTask<T> DeserializeAsync<T>(byte[] buffer, CancellationToken token = default);
        ValueTask<T> DeserializeAsync<T>(Stream stream, CancellationToken token = default);
    }
}