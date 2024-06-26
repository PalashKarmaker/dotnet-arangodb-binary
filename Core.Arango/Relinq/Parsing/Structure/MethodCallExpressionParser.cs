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

using Core.Arango.Relinq.Parsing.ExpressionVisitors;
using Core.Arango.Relinq.Parsing.Structure.IntermediateModel;
using Core.Arango.Relinq.Utilities;
using Remotion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Core.Arango.Relinq.Parsing.Structure
{
    /// <summary>
    ///     Parses a <see cref="MethodCallExpression" /> and creates an <see cref="IExpressionNode" /> from it. This is used by
    ///     <see cref="ExpressionTreeParser" /> for parsing whole expression trees.
    /// </summary>
    internal sealed class MethodCallExpressionParser
    {
        private readonly INodeTypeProvider _nodeTypeProvider;

        public MethodCallExpressionParser(INodeTypeProvider nodeTypeProvider)
        {
            ArgumentUtility.CheckNotNull("nodeTypeProvider", nodeTypeProvider);
            _nodeTypeProvider = nodeTypeProvider;
        }

        public IExpressionNode Parse(
            string associatedIdentifier, IExpressionNode source, IEnumerable<Expression> arguments,
            MethodCallExpression expressionToParse)
        {
            ArgumentUtility.CheckNotNullOrEmpty("associatedIdentifier", associatedIdentifier);
            ArgumentUtility.CheckNotNull("source", source);
            ArgumentUtility.CheckNotNull("expressionToParse", expressionToParse);
            ArgumentUtility.CheckNotNull("arguments", arguments);

            var nodeType = GetNodeType(expressionToParse);
            var additionalConstructorParameters = arguments.Select(ProcessArgumentExpression).ToArray();

            var parseInfo = new MethodCallExpressionParseInfo(associatedIdentifier, source, expressionToParse);
            return CreateExpressionNode(nodeType, parseInfo, additionalConstructorParameters);
        }

        private Type GetNodeType(MethodCallExpression expressionToParse)
        {
            var nodeType = _nodeTypeProvider.GetNodeType(expressionToParse.Method);
            if (nodeType == null)
                throw CreateParsingErrorException(
                    expressionToParse,
                    "This overload of the method '{0}.{1}' is currently not supported.",
                    expressionToParse.Method.DeclaringType.FullName,
                    expressionToParse.Method.Name);
            return nodeType;
        }

        private Expression ProcessArgumentExpression(Expression argumentExpression)
        {
            // First, convert the argument expressions to their actual values - this unwraps ConstantantExpressions and UnaryExpressions
            var convertedParameters = UnwrapArgumentExpression(argumentExpression);
            // Then, detect subqueries
            var parametersWithSubQueriesDetected =
                SubQueryFindingExpressionVisitor.Process(convertedParameters, _nodeTypeProvider);

            return parametersWithSubQueriesDetected;
        }


        private Expression UnwrapArgumentExpression(Expression expression)
        {
            // Each argument of a MethodCallExpression will either be a UnaryExpression/Quote, which represents an expression passed to a Queryable method,
            // a LambdaExpression, which represents an expression passed to an Enumerable method,
            // a ConstantExpression that contains the expression passed to the method,
            // or any other expression that represents a constant passed to the method.
            // We only support the former three, to support the latter, PartialEvaluatingExpressionTreeVisitor must be used.

            if (expression.NodeType == ExpressionType.Quote)
                return ((UnaryExpression)expression).Operand;
            if (expression.NodeType == ExpressionType.Constant &&
                ((ConstantExpression)expression).Value is LambdaExpression)
                return (Expression)((ConstantExpression)expression).Value;
            return expression;
        }

        private IExpressionNode CreateExpressionNode(Type nodeType, MethodCallExpressionParseInfo parseInfo,
            object[] additionalConstructorParameters)
        {
            try
            {
                return MethodCallExpressionNodeFactory.CreateExpressionNode(nodeType, parseInfo,
                    additionalConstructorParameters);
            }
            catch (ExpressionNodeInstantiationException ex)
            {
                throw CreateParsingErrorException(parseInfo.ParsedExpression, "{0}", ex.Message);
            }
        }

        private NotSupportedException CreateParsingErrorException(MethodCallExpression expression, string message,
            params object[] args)
        {
            return new(
                string.Format("Could not parse expression '{0}': ", expression.BuildString())
                + string.Format(message, args));
        }
    }
}