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
    ///     Represents the main data source in a query, producing data items that are filtered, aggregated, projected, or
    ///     otherwise processed by
    ///     subsequent clauses.
    /// </summary>
    /// <example>
    ///     In C#, the first "from" clause in the following sample corresponds to the <see cref="MainFromClause" />:
    ///     <ode>
    ///         var query = from s in Students
    ///         from f in s.Friends
    ///         select f;
    ///     </ode>
    /// </example>
    internal sealed class MainFromClause : FromClauseBase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="MainFromClause" /> class.
        /// </summary>
        /// <param name="itemName">A name describing the items generated by the from clause.</param>
        /// <param name="itemType">The type of the items generated by the from clause.</param>
        /// <param name="fromExpression">The <see cref="Expression" /> generating data items for this from clause.</param>
        public MainFromClause(string itemName, Type itemType, Expression fromExpression)
            : base(
                ArgumentUtility.CheckNotNullOrEmpty("itemName", itemName),
                ArgumentUtility.CheckNotNull("itemType", itemType),
                ArgumentUtility.CheckNotNull("fromExpression", fromExpression))
        {
        }

        /// <summary>
        ///     Accepts the specified visitor by calling its <see cref="IQueryModelVisitor.VisitMainFromClause" /> method.
        /// </summary>
        /// <param name="visitor">The visitor to accept.</param>
        /// <param name="queryModel">The query model in whose context this clause is visited.</param>
        public void Accept(IQueryModelVisitor visitor, QueryModel queryModel)
        {
            ArgumentUtility.CheckNotNull("visitor", visitor);
            ArgumentUtility.CheckNotNull("queryModel", queryModel);

            visitor.VisitMainFromClause(this, queryModel);
        }

        /// <summary>
        ///     Clones this clause, registering its clone with the <paramref name="cloneContext" />.
        /// </summary>
        /// <param name="cloneContext">The clones of all query source clauses are registered with this <see cref="CloneContext" />.</param>
        /// <returns>A clone of this clause.</returns>
        public MainFromClause Clone(CloneContext cloneContext)
        {
            ArgumentUtility.CheckNotNull("cloneContext", cloneContext);

            var clone = new MainFromClause(ItemName, ItemType, FromExpression);
            cloneContext.QuerySourceMapping.AddMapping(this, new QuerySourceReferenceExpression(clone));
            return clone;
        }
    }
}