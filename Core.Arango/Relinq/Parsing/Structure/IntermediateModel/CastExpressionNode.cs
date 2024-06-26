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

using Core.Arango.Relinq.Clauses;
using Core.Arango.Relinq.Clauses.ResultOperators;
using Core.Arango.Relinq.Parsing.ExpressionVisitors;
using Core.Arango.Relinq.Utilities;
using Remotion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Core.Arango.Relinq.Parsing.Structure.IntermediateModel
{
    /// <summary>
    ///     Represents a <see cref="MethodCallExpression" /> for
    ///     <see cref="Queryable.Cast{TResult}" />.
    ///     It is generated by <see cref="ExpressionTreeParser" /> when an <see cref="Expression" /> tree is parsed.
    /// </summary>
    internal sealed class CastExpressionNode : ResultOperatorExpressionNodeBase
    {
        public CastExpressionNode(MethodCallExpressionParseInfo parseInfo)
            : base(parseInfo, null, null)
        {
            if (!parseInfo.ParsedExpression.Method.IsGenericMethod ||
                parseInfo.ParsedExpression.Method.GetGenericArguments().Length != 1)
                throw new ArgumentException("The parsed method must have exactly one generic argument.", "parseInfo");
        }

        public Type CastItemType => ParsedExpression.Method.GetGenericArguments()[0];

        public static IEnumerable<MethodInfo> GetSupportedMethods()
        {
            return ReflectionUtility.EnumerableAndQueryableMethods.WhereNameMatches("Cast");
        }

        public override Expression Resolve(
            ParameterExpression inputParameter, Expression expressionToBeResolved,
            ClauseGenerationContext clauseGenerationContext)
        {
            ArgumentUtility.CheckNotNull("inputParameter", inputParameter);
            ArgumentUtility.CheckNotNull("expressionToBeResolved", expressionToBeResolved);

            var convertExpression = Expression.Convert(inputParameter, CastItemType);
            var expressionWithCast =
                ReplacingExpressionVisitor.Replace(inputParameter, convertExpression, expressionToBeResolved);
            return Source.Resolve(inputParameter, expressionWithCast, clauseGenerationContext);
        }

        protected override ResultOperatorBase CreateResultOperator(ClauseGenerationContext clauseGenerationContext)
        {
            var castItemType = CastItemType;
            return new CastResultOperator(castItemType);
        }
    }
}