﻿using Core.Arango.Linq.Data;
using Core.Arango.Linq.Interface;
using Core.Arango.Relinq;
using Core.Arango.Relinq.Parsing.Structure;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Core.Arango.Linq.Query
{
    internal sealed class ArangoQueryProvider : QueryProviderBase
    {
        private readonly IArangoLinq db;

        /// <summary>
        ///     Initializes a new instance of <see cref="ArangoQueryProvider" /> using a custom <see cref="IQueryParser" />.
        /// </summary>
        public ArangoQueryProvider(Type queryableType, IQueryParser queryParser, IQueryExecutor executor,
            IArangoLinq db)
            : base(LinqUtility.CheckNotNull("queryParser", queryParser), LinqUtility.CheckNotNull("executor", executor))
        {
            LinqUtility.CheckNotNull("queryableType", queryableType);
            CheckQueryableType(queryableType);

            QueryableType = queryableType;
            this.db = db;
        }

        /// <summary>
        ///     Gets the type of queryable created by this provider. This is the generic type definition of an implementation of
        ///     <see cref="IQueryable{T}" />
        ///     (usually a subclass of <see cref="QueryableBase{T}" />) with exactly one type argument.
        /// </summary>
        public Type QueryableType { get; }

        private void CheckQueryableType(Type queryableType)
        {
            LinqUtility.CheckTypeIsAssignableFrom("queryableType", queryableType, typeof(IQueryable));

            var queryableTypeInfo = queryableType.GetTypeInfo();
            if (!queryableTypeInfo.IsGenericTypeDefinition)
            {
                var message = string.Format(
                    "Expected the generic type definition of an implementation of IQueryable<T>, but was '{0}'.",
                    queryableType);
                throw new ArgumentException(message, "queryableType");
            }

            var genericArgumentCount = queryableTypeInfo.GenericTypeParameters.Length;
            if (genericArgumentCount != 1)
            {
                var message = string.Format(
                    "Expected the generic type definition of an implementation of IQueryable<T> with exactly one type argument, but found {0} arguments on '{1}.",
                    genericArgumentCount,
                    queryableType);
                throw new ArgumentException(message, "queryableType");
            }
        }

        /// <summary>
        ///     Creates a new <see cref="IQueryable" /> (of type <see cref="QueryableType" /> with <typeparamref name="T" /> as its
        ///     generic argument) that
        ///     represents the query defined by <paramref name="expression" /> and is able to enumerate its results.
        /// </summary>
        /// <typeparam name="T">The type of the data items returned by the query.</typeparam>
        /// <param name="expression">
        ///     An expression representing the query for which a <see cref="IQueryable{T}" /> should be
        ///     created.
        /// </param>
        /// <returns>An <see cref="IQueryable{T}" /> that represents the query defined by <paramref name="expression" />.</returns>
        public override IQueryable<T> CreateQuery<T>(Expression expression)
        {
            return (IQueryable<T>)Activator.CreateInstance(QueryableType.MakeGenericType(typeof(T)), this, expression,
                db);
        }

        public QueryData GetQueryData(Expression expression)
        {
            LinqUtility.CheckNotNull("expression", expression);

            var queryModel = GenerateQueryModel(expression);

            var visitor = new ArangoModelVisitor(db);
            visitor.VisitQueryModel(queryModel);

            visitor.QueryData.Query = visitor.QueryText.ToString();

            return visitor.QueryData;
        }
    }
}