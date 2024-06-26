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
using Remotion.Utilities;
using System;
using System.Linq.Expressions;

namespace Core.Arango.Relinq.Clauses
{
    /// <summary>
    ///     Represents a data source in a query that adds new data items in addition to those provided by the
    ///     <see cref="MainFromClause" />.
    /// </summary>
    /// <example>
    ///     In C#, the second "from" clause in the following sample corresponds to an <see cref="AdditionalFromClause" />:
    ///     <ode>
    ///         var query = from s in Students
    ///         from f in s.Friends
    ///         select f;
    ///     </ode>
    /// </example>
    internal sealed class AdditionalFromClause : FromClauseBase, IBodyClause
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="AdditionalFromClause" /> class.
        /// </summary>
        /// <param name="itemName">A name describing the items generated by the from clause.</param>
        /// <param name="itemType">The type of the items generated by the from clause.</param>
        /// <param name="fromExpression">The <see cref="Expression" /> generating the items of this from clause.</param>
        public AdditionalFromClause(string itemName, Type itemType, Expression fromExpression)
            : base(
                ArgumentUtility.CheckNotNullOrEmpty("itemName", itemName),
                ArgumentUtility.CheckNotNull("itemType", itemType),
                ArgumentUtility.CheckNotNull("fromExpression", fromExpression))
        {
        }

        /// <summary>
        ///     Accepts the specified visitor by calling its <see cref="IQueryModelVisitor.VisitAdditionalFromClause" /> method.
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

            visitor.VisitAdditionalFromClause(this, queryModel, index);
        }

        IBodyClause IBodyClause.Clone(CloneContext cloneContext)
        {
            return Clone(cloneContext);
        }

        /// <summary>
        ///     Clones this clause, registering its clone with the <paramref name="cloneContext" />.
        /// </summary>
        /// <param name="cloneContext">The clones of all query source clauses are registered with this <see cref="CloneContext" />.</param>
        /// <returns>A clone of this clause.</returns>
        public AdditionalFromClause Clone(CloneContext cloneContext)
        {
            ArgumentUtility.CheckNotNull("cloneContext", cloneContext);

            var clone = new AdditionalFromClause(ItemName, ItemType, FromExpression);
            cloneContext.QuerySourceMapping.AddMapping(this, new QuerySourceReferenceExpression(clone));
            return clone;
        }
    }
}