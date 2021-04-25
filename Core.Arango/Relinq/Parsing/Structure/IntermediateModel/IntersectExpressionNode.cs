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

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Core.Arango.Relinq.Clauses;
using Core.Arango.Relinq.Clauses.ResultOperators;
using Core.Arango.Relinq.Utilities;
using Remotion.Utilities;

namespace Core.Arango.Relinq.Parsing.Structure.IntermediateModel
{
    /// <summary>
    ///     Represents a <see cref="MethodCallExpression" /> for
    ///     <see
    ///         cref="Queryable.Intersect{TSource}(System.Linq.IQueryable{TSource},System.Collections.Generic.IEnumerable{TSource})" />
    ///     .
    ///     It is generated by <see cref="ExpressionTreeParser" /> when an <see cref="Expression" /> tree is parsed.
    ///     When this node is used, it usually follows (or replaces) a <see cref="SelectExpressionNode" /> of an
    ///     <see cref="IExpressionNode" /> chain that
    ///     represents a query.
    /// </summary>
    internal sealed class IntersectExpressionNode : ResultOperatorExpressionNodeBase
    {
        public IntersectExpressionNode(MethodCallExpressionParseInfo parseInfo, Expression source2)
            : base(parseInfo, null, null)
        {
            ArgumentUtility.CheckNotNull("source2", source2);
            Source2 = source2;
        }

        public Expression Source2 { get; }

        public static IEnumerable<MethodInfo> GetSupportedMethods()
        {
            return ReflectionUtility.EnumerableAndQueryableMethods.WhereNameMatches("Intersect")
                .WithoutEqualityComparer();
        }

        public override Expression Resolve(ParameterExpression inputParameter, Expression expressionToBeResolved,
            ClauseGenerationContext clauseGenerationContext)
        {
            ArgumentUtility.CheckNotNull("inputParameter", inputParameter);
            ArgumentUtility.CheckNotNull("expressionToBeResolved", expressionToBeResolved);

            // this simply streams its input data to the output without modifying its structure, so we resolve by passing on the data to the previous node
            return Source.Resolve(inputParameter, expressionToBeResolved, clauseGenerationContext);
        }

        protected override ResultOperatorBase CreateResultOperator(ClauseGenerationContext clauseGenerationContext)
        {
            return new IntersectResultOperator(Source2);
        }
    }
}