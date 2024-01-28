﻿using Core.Arango.Protocol;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Arango.Modules.Internal
{
    internal class ArangoPregelModule : ArangoModule, IArangoPregelModule
    {
        internal ArangoPregelModule(IArangoContext context) : base(context)
        {
        }

        public async Task<string> StartJobAsync(ArangoHandle database, ArangoPregel job,
            CancellationToken cancellationToken = default)
        {
            return await SendAsync<string>(database, HttpMethod.Post,
                ApiPath(database, "control_pregel"),
                job, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        public async Task<ArangoPregelStatus> GetJobStatusAsync(ArangoHandle database, string id,
            CancellationToken cancellationToken = default)
        {
            return await SendAsync<ArangoPregelStatus>(database, HttpMethod.Get,
                ApiPath(database, $"control_pregel/{id}"),
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        public async Task DeleteJobAsync(ArangoHandle database, string id,
            CancellationToken cancellationToken = default)
        {
            await SendAsync<ArangoVoid>(database, HttpMethod.Delete,
                ApiPath(database, $"control_pregel/{id}"),
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
}