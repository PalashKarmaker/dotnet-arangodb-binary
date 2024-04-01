using Core.Arango.Linq.Interface;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Arango.Linq;

internal class ArangoLinq(IArangoContext context, ArangoHandle handle) : IArangoLinq
{
    private readonly IArangoContext _context = context;
    private readonly ArangoHandle _handle = handle;

    public string ResolvePropertyName(Type t, string s) => _context.Configuration.ResolveProperty(t, s);

    public string ResolveCollectionName(Type t) => _context.Configuration.ResolveCollection(t);

    public Func<string, string> TranslateGroupByIntoName => _context.Configuration.ResolveGroupBy;

    public IAsyncEnumerable<T> StreamAsync<T>(string query, IDictionary<string, object> bindVars, CancellationToken cancellationToken = default) => _context.Query.ExecuteStreamAsync<T>(_handle, query, bindVars, cancellationToken: cancellationToken);

    public async Task<ArangoList<T>> ExecuteAsync<T>(string query, IDictionary<string, object> bindVars, CancellationToken cancellationToken = default) => await _context.Query.ExecuteAsync<T>(_handle, query, bindVars, cancellationToken: cancellationToken).ConfigureAwait(false);
}