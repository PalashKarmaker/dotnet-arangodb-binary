﻿using Core.Arango.Linq.Data;
using Core.Arango.Linq.Interface;
using Core.Arango.Relinq;
using Core.Arango.Relinq.Parsing.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Arango.Linq.Query
{
    internal class ArangoQueryable<T> : QueryableBase<T>, IAqlModifiable<T>,
        ITraversalQueryable<T>,
        IShortestPathQueryable<T>
    {
        private readonly IArangoLinq db;

        public ArangoQueryable(IQueryParser queryParser, IQueryExecutor executor, IArangoLinq db)
            : base(new ArangoQueryProvider(typeof(ArangoQueryable<>), queryParser, executor, db))
        {
            this.db = db;
        }

        public ArangoQueryable(IQueryProvider provider, Expression expression, IArangoLinq db)
            : base(provider, expression)
        {
            this.db = db;
        }

        public async Task<List<T>> ToListAsync(CancellationToken cancellationToken = default)
        {
            var data = GetQueryData();
            return await db.ExecuteAsync<T>(data.Query, data.BindVars, cancellationToken).ConfigureAwait(false);
        }

        public async Task<T> SingleOrDefaultAsync(CancellationToken cancellationToken = default)
        {
            var data = GetQueryData();
            var list = await db.ExecuteAsync<T>(data.Query, data.BindVars, cancellationToken).ConfigureAwait(false);
            return list.SingleOrDefault();
        }

        public async Task<T> SingleAsync(CancellationToken cancellationToken = default)
        {
            var data = GetQueryData();
            var list = await db.ExecuteAsync<T>(data.Query, data.BindVars, cancellationToken).ConfigureAwait(false);
            return list.Single();
        }

        public async Task<T> FirstOrDefaultAsync(CancellationToken cancellationToken = default)
        {
            var data = GetQueryData();
            var list = await db.ExecuteAsync<T>(data.Query, data.BindVars, cancellationToken).ConfigureAwait(false);
            return list.FirstOrDefault();
        }

        public async Task<T> FirstAsync(CancellationToken cancellationToken = default)
        {
            var data = GetQueryData();
            var list = await db.ExecuteAsync<T>(data.Query, data.BindVars, cancellationToken).ConfigureAwait(false);
            return list.First();
        }

        public QueryData GetQueryData()
        {
            var arangoQueryProvider = Provider as ArangoQueryProvider;

            if (arangoQueryProvider == null)
                throw new NotSupportedException("ArangoQueryable should be use with ArangoQueryProvider");

            return arangoQueryProvider.GetQueryData(Expression);
        }
    }
}