using Core.Arango.Protocol;
using Core.Arango.Protocol.Internal;
using Core.Arango.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Arango.Transport;

/// <summary>
///     Arango HTTP 1.1/2.0 Transport Implementation
/// </summary>
public class ArangoHttpTransport(IArangoConfiguration configuration) : IArangoTransport
{
    private readonly HttpClient httpClient = configuration.HttpClient ?? new();
    private string _auth = "";
    private DateTime _authValidUntil = DateTime.MinValue;

    /// <inheritdoc />
    protected string BasicAuth
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_auth) && !string.IsNullOrWhiteSpace(configuration.User) && !string.IsNullOrWhiteSpace(configuration.Password))
                _auth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{configuration.User}:{configuration.Password}"));
            return _auth;
        }
    }

    private async Task Authenticate(bool auth, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(configuration.User))
            return;

        if (auth && (_auth == null || _authValidUntil < DateTime.UtcNow))
        {
            var authResponse = await SendAsync<AuthResponse>(HttpMethod.Post,
                "/_open/auth",
                new AuthRequest
                {
                    Username = configuration.User,
                    Password = configuration.Password ?? string.Empty
                }, auth: false, cancellationToken: cancellationToken).ConfigureAwait(false);

            var jwt = authResponse.Jwt;
            var token = new JwtSecurityToken(jwt.Replace("=", ""));
            _auth = $"Bearer {jwt}";
            _authValidUntil = token.ValidTo.AddMinutes(-5);
        }
    }

    /// <inheritdoc />
    public async Task<T> SendAsync<T>(HttpMethod m, string url, object body = null,
        string transaction = null, bool throwOnError = true, bool auth = true,
        IDictionary<string, string> headers = null,
        CancellationToken cancellationToken = default)
    {
        auth = false;
        if (auth)
            await Authenticate(auth, cancellationToken).ConfigureAwait(false);

        do
        {
            var req = new HttpRequestMessage(m, configuration.Server + url);
            ApplyHeaders(transaction, auth, req, headers);
            //var json = ToJson(configuration, body);
            //if (!string.IsNullOrEmpty(json))
            //    req.Content = new StringContent(json, Encoding.UTF8, "application/json");
            var data = ToByteArray(configuration, body);
            if (data != null && data.Length > 0)
                req.Content = new ByteArrayContent(data);
            else
                req.Headers.Add(HttpRequestHeader.ContentLength.ToString(), "0");
            SetBasicAuth(httpClient);
            var (retryMax, retryCount) = (5, 0);
            try
            {
                return await GetResponse<T>(configuration.Serializer, req, cancellationToken).ConfigureAwait(false);
            }
            catch (ArangoException exc)
            {
                if (++retryCount >= retryMax)
                    throw;
                await Task.Delay(500, cancellationToken).ConfigureAwait(false);
            }
        } while (true);
    }

    private static byte[] ToByteArray(IArangoConfiguration configuration, object body)
    {
        if (body == null)
            return [];
        var json = configuration.Serializer.Serialize(body);
        if (!string.IsNullOrWhiteSpace(json))
            return Encoding.UTF8.GetBytes(json);
        return [];
    }
    private static string ToJson(IArangoConfiguration configuration, object body)
    {
        if (body == null)
            return string.Empty;
        return configuration.Serializer.Serialize(body);
    }

    private async Task<T> GetResponse<T>(IArangoSerializer serializer, HttpRequestMessage req, CancellationToken cancellationToken)
    {
        var res = await httpClient.SendAsync(req, cancellationToken).ConfigureAwait(false);
        var content = await res.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!res.IsSuccessStatusCode)
        {
            var error = serializer.Deserialize<ErrorResponse>(content);
            throw new ArangoException(content, error.ErrorMessage,
                (HttpStatusCode)error.Code, (ArangoErrorCode)error.ErrorNum);
        }
        if (res.Headers.Contains("X-Arango-Error-Codes"))
        {
            var errors = configuration.Serializer.Deserialize<IEnumerable<ErrorResponse>>(content)
                .Select(error => new ArangoError(error.ErrorMessage, (ArangoErrorCode)error.ErrorNum));
            throw new ArangoException("HeaderError: " + content, errors);
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
        await Authenticate(auth, cancellationToken).ConfigureAwait(false);

        var req = new HttpRequestMessage(m, configuration.Server + url);
        ApplyHeaders(transaction, auth, req, headers);

        if (body != null)
        {
            var json = configuration.Serializer.Serialize(body);
            req.Content = new ByteArrayContent(Encoding.UTF8.GetBytes(json));
        }
        else
            req.Headers.Add(HttpRequestHeader.ContentLength.ToString(), "0");
        SetBasicAuth(httpClient);
        var res = await httpClient.SendAsync(req, cancellationToken).ConfigureAwait(false);
        var content = await res.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        if (!res.IsSuccessStatusCode)
            if (throwOnError)
                throw new ArangoException(content);
            else return default;

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
        await Authenticate(auth, cancellationToken).ConfigureAwait(false);

        var req = new HttpRequestMessage(m, configuration.Server + url);
        ApplyHeaders(transaction, auth, req, headers);
        req.Content = body;
        SetBasicAuth(httpClient);
        var res = await httpClient.SendAsync(req, cancellationToken);

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
    protected void SetBasicAuth(HttpClient client) => client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", BasicAuth);

    private void ApplyHeaders(string transaction, bool auth, HttpRequestMessage msg,
        IDictionary<string, string> headers)
    {
        msg.Headers.Add(HttpRequestHeader.KeepAlive.ToString(), "true");

        if (auth && !string.IsNullOrWhiteSpace(_auth))
            msg.Headers.Add(HttpRequestHeader.Authorization.ToString(), _auth);

        if (transaction != null)
            msg.Headers.Add("x-arango-trx-id", transaction);

        if (configuration.AllowDirtyRead)
            msg.Headers.Add("x-arango-allow-dirty-read", "true");

        if (headers != null)
            foreach (var header in headers)
                msg.Headers.Add(header.Key, header.Value);
    }

    private class AuthRequest
    {
        [JsonProperty("username")]
        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        [JsonPropertyName("password")]
        public string Password { get; set; }
    }

    private class AuthResponse
    {
        [JsonProperty("jwt")]
        [JsonPropertyName("jwt")]
        public string Jwt { get; set; }
    }
}