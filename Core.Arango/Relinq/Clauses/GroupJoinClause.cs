// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 

using Core.Arango.Relinq.Clauses.Expressions;
using Core.Arango.Relinq.Utilities;
using Remotion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Core.Arango.Relinq.Clauses
{
    /// <summary>
    ///     Represents the join part of a query, adding new data items and joining them with data items from previous clauses.
    ///     In contrast to
    ///     <see cref="Clauses.JoinClause" />, the <see cref="GroupJoinClause" /> does not provide access to the individual
    ///     items of the joined query source.
    ///     Instead, it provides access to all joined items for each item coming from the previous clauses, thus grouping them
    ///     together. The semantics
    ///     of this join is so that for all input items, a joined sequence is returned. That sequence can be empty if no joined
    ///     items are available.
    /// </summary>
    /// <example>
    ///     In C#, the "into" clause in the following sample corresponds to a <see cref="GroupJoinClause" />. The "join" part
    ///     before that is encapsulated
    ///     as a <see cref="Clauses.JoinClause" /> held in <see cref="JoinClause" />. The <see cref="JoinClause" /> adds a new
    ///     query source to the query
    ///     ("addresses"), but the item type of that query source is <see cref="IEnumerable{T}" />, not "Address". Therefore,
    ///     it can be
    ///     used in the <see cref="FromClauseBase.FromExpression" /> of an <see cref="AdditionalFromClause" /> to extract the
    ///     single items.
    ///     <code>
    /// var query = from s in Students
    ///             join a in Addresses on s.AdressID equals a.ID into addresses
    ///             from a in addresses
    ///             select new { s, a };
    /// </code>
    /// </example>
    internal sealed class GroupJoinClause : IQuerySource, IBodyClause
    {
        private string _itemName;
        private Type _itemType;
        private JoinClause _joinClause;

        public GroupJoinClause(string itemName, Type itemType, JoinClause joinClause)
        {
            ArgumentUtility.CheckNotNullOrEmpty("itemName", itemName);
            ArgumentUtility.CheckNotNull("itemType", itemType);
            ArgumentUtility.CheckNotNull("joinClause", joinClause);

            ItemName = itemName;
            ItemType = itemType;
            JoinClause = joinClause;
        }

        /// <summary>
        ///     Gets or sets the inner join clause of this <see cref="GroupJoinClause" />. The <see cref="JoinClause" /> represents
        ///     the actual join operation
        ///     performed by this clause; its results are then grouped by this clause before streaming them to subsequent clauses.
        ///     <see cref="QuerySourceReferenceExpression" /> objects outside the <see cref="GroupJoinClause" /> must not point to
        ///     <see cref="JoinClause" />
        ///     because the items generated by it are only available in grouped form from outside this clause.
        /// </summary>
        public JoinClause JoinClause
        {
            get => _joinClause;
            set => _joinClause = ArgumentUtility.CheckNotNull("value", value);
        }

        /// <summary>
        ///     Transforms all the expressions in this clause and its child objects via the given
        ///     <paramref name="transformation" /> delegate.
        /// </summary>
        /// <param name="transformation">
        ///     The transformation object. This delegate is called for each <see cref="Expression" /> within this
        ///     clause, and those expressions will be replaced with what the delegate returns.
        /// </param>
        public void TransformExpressions(Func<Expression, Expression> transformation)
        {
            ArgumentUtility.CheckNotNull("transformation", transformation);
            JoinClause.TransformExpressions(transformation);
        }

        /// <summary>
        ///     Accepts the specified visitor by calling its <see cref="IQueryModelVisitor.VisitGroupJoinClause" /> method.
        /// </summary>
        /// <param name="visitor">The visitor to accept.</param>
        /// <param name="queryModel">The query model in whose context this clause is visited.</param>
        /// <param name="index">
        ///     The index of this clause in the <paramref name="queryModel" />'s
        ///     <see cref="QueryModel.BodyClauses" /> collection.
        /// </param>
        public void Accept(IQueryModelVisitor visitor, QueryModel queryModel, int index)
        {
            ArgumentUtility.CheckNotNull("visitor", visitor);
            ArgumentUtility.CheckNotNull("queryModel", queryModel);

            visitor.VisitGroupJoinClause(this, queryModel, index);
        }

        IBodyClause IBodyClause.Clone(CloneContext cloneContext)
        {
            return Clone(cloneContext);
        }

        /// <summary>
        ///     Gets or sets a name describing the items generated by this <see cref="GroupJoinClause" />.
        /// </summary>
        /// <remarks>
        ///     Item names are inferred when a query expression is parsed, and they usually correspond to the variable names
        ///     present in that expression.
        ///     However, note that names are not necessarily unique within a <see cref="QueryModel" />. Use names only for
        ///     readability and debugging, not for
        ///     uniquely identifying <see cref="IQuerySource" /> objects. To match an <see cref="IQuerySource" /> with its
        ///     references, use the
        ///     <see cref="QuerySourceReferenceExpression.ReferencedQuerySource" /> property rather than the
        ///     <see cref="ItemName" />.
        /// </remarks>
        public string ItemName
        {
            get => _itemName;
            set => _itemName = ArgumentUtility.CheckNotNull("value", value);
        }

        /// <summary>
        ///     Gets or sets the type of the items generated by this <see cref="GroupJoinClause" />. This must implement
        ///     <see cref="IEnumerable{T}" />.
        /// </summary>
        /// <note type="warning">
        ///     Changing the <see cref="ItemType" /> of a <see cref="IQuerySource" /> can make all
        ///     <see cref="QuerySourceReferenceExpression" /> objects that
        ///     point to that <see cref="IQuerySource" /> invalid, so the property setter should be used with care.
        /// </note>
        public Type ItemType
        {
            get => _itemType;
            set
            {
                ArgumentUtility.CheckNotNull("value", value);
                ReflectionUtility.CheckTypeIsClosedGenericIEnumerable(value, "value");

                _itemType = value;
            }
        }

        /// <summary>
        ///     Clones this clause, registering its clone with the <paramref name="cloneContext" />.
        /// </summary>
        /// <param name="cloneContext">The clones of all query source clauses are registered with this <see cref="CloneContext" />.</param>
        /// <returns>A clone of this clause.</returns>
        public GroupJoinClause Clone(CloneContext cloneContext)
        {
            ArgumentUtility.CheckNotNull("cloneContext", cloneContext);

            var clone = new GroupJoinClause(ItemName, ItemType, JoinClause.Clone(cloneContext));
            cloneContext.QuerySourceMapping.AddMapping(this, new QuerySourceReferenceExpression(clone));
            return clone;
        }

        public override string ToString()
        {
            return string.Format("{0} into {1} {2}", JoinClause, ItemType.Name, ItemName);
        }
    }
}