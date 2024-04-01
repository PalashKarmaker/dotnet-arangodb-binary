using Core.Arango.Protocol;
using Core.Arango.Protocol.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Arango.Transport;

/// <summary>
///     Arango HTTP 1.1/2.0 Transport Implementation
/// </summary>
public class ArangoHttpTransport(IArangoConfiguration configuration) : IArangoTransport
{
    private static readonly HttpClient DefaultHttpClient = new();
    string _auth = "";
    /// <inheritdoc />
    protected string Auth
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_auth) && !string.IsNullOrWhiteSpace(configuration.User) && !string.IsNullOrWhiteSpace(configuration.Password))
                _auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{configuration.User}:{configuration.Password}"));
            return _auth;
        }
    }

    /// <inheritdoc />
    public async Task<T> SendAsync<T>(HttpMethod m, string url, object body = null,
        string transaction = null, bool throwOnError = true, bool auth = true,
        IDictionary<string, string> headers = null,
        CancellationToken cancellationToken = default)
    {
        using var req = new HttpRequestMessage(m, configuration.Server + url);
        ApplyHeaders(transaction, auth, req, headers);

        if (body != null)
        {
            var json = configuration.Serializer.Serialize(body);
            req.Content = new ByteArrayContent(Encoding.UTF8.GetBytes(json));
        }
        else
            req.Headers.Add(HttpRequestHeader.ContentLength.ToString(), "0");
        var httpClient = DefaultHttpClient;
        SetBasicAuth(httpClient);
        using var res = await httpClient.SendAsync(req, cancellationToken).ConfigureAwait(false);

        if (!res.IsSuccessStatusCode)
            if (throwOnError)
            {
                var errorContent = await res.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                var error = configuration.Serializer.Deserialize<ErrorResponse>(errorContent);
                throw new ArangoException(errorContent, error.ErrorMessage,
                    (HttpStatusCode)error.Code, (ArangoErrorCode)error.ErrorNum);
            }
            else
                return default;

        var content = await res.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (res.Headers.Contains("X-Arango-Error-Codes"))
        {
            var errors = configuration.Serializer.Deserialize<IEnumerable<ErrorResponse>>(content)
                .Select(error => new ArangoError(error.ErrorMessage, (ArangoErrorCode)error.ErrorNum));
            throw new ArangoException(content, errors);
        }

        if (content == "{}" || string.IsNullOrWhiteSpace(content))
            return default;

        return configuration.Serializer.Deserialize<T>(content);
    }

    /// <inheritdoc />
    public async Task<object> SendAsync(Type type, HttpMethod m, string url, object body = null,
        string transaction = null, bool throwOnError = true, bool auth = true,
        IDictionary<string, string> headers = null,
        CancellationToken cancellationToken = default)
    {
        using var req = new HttpRequestMessage(m, configuration.Server + url);
        ApplyHeaders(transaction, auth, req, headers);

        if (body != null)
        {
            var json = configuration.Serializer.Serialize(body);
            req.Content = new ByteArrayContent(Encoding.UTF8.GetBytes(json));
        }
        else
            req.Headers.Add(HttpRequestHeader.ContentLength.ToString(), "0");
        var httpClient = DefaultHttpClient;
        SetBasicAuth(httpClient);
        using var res = await httpClient.SendAsync(req, cancellationToken).ConfigureAwait(false);

        if (!res.IsSuccessStatusCode)
            if (throwOnError)
                throw new ArangoException(await res.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
            else return default;

        var content = await res.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (content == "{}" || string.IsNullOrWhiteSpace(content))
            return default;

        return configuration.Serializer.Deserialize(content, type);
    }

    /// <inheritdoc />
    public async Task<HttpContent> SendContentAsync(HttpMethod m, string url, HttpContent body = null,
        string transaction = null,
        bool throwOnError = true, bool auth = true, IDictionary<string, string> headers = null,
        CancellationToken cancellationToken = default)
    {
        using var req = new HttpRequestMessage(m, configuration.Server + url);
        ApplyHeaders(transaction, auth, req, headers);
        req.Content = body;
        var httpClient = DefaultHttpClient;
        SetBasicAuth(httpClient);
        using var res = await httpClient.SendAsync(req, cancellationToken);

        if (!res.IsSuccessStatusCode && throwOnError)
        {
            var errorContent = await res.Content.ReadAsStringAsync(cancellationToken);
            var error = configuration.Serializer.Deserialize<ErrorResponse>(errorContent);
            throw new ArangoException(errorContent, error.ErrorMessage,
                (HttpStatusCode)error.Code, (ArangoErrorCode)error.ErrorNum);
        }
        return res.Content;
    }

    /// <summary>
    /// When using Basic auth, call this method to set the username and password
    /// used in requests to ArangoDB.
    /// </summary>
    /// <param name="client"></param>
    protected void SetBasicAuth(HttpClient client) => client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Auth);

    private void ApplyHeaders(string transaction, bool auth, HttpRequestMessage msg,
        IDictionary<string, string> headers)
    {
        msg.Headers.Add(HttpRequestHeader.KeepAlive.ToString(), "true");

        /*if (auth && !string.IsNullOrWhiteSpace(_auth))
            msg.Headers.Add(HttpRequestHeader.Authorization.ToString(), _auth);*/

        if (transaction != null)
            msg.Headers.Add("x-arango-trx-id", transaction);

        if (configuration.AllowDirtyRead)
            msg.Headers.Add("x-arango-allow-dirty-read", "true");

        if (headers != null)
            foreach (var header in headers)
                msg.Headers.Add(header.Key, header.Value);
    }
}