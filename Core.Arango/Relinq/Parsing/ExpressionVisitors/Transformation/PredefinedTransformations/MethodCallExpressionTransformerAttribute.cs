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

using Remotion.Utilities;
using System;
using System.Linq.Expressions;

namespace Core.Arango.Relinq.Parsing.ExpressionVisitors.Transformation.PredefinedTransformations
{
    /// <summary>
    ///     Chooses a given <see cref="IExpressionTransformer{T}" /> for a specific method (or property get accessor).
    /// </summary>
    /// <remarks>
    ///     The <see cref="IExpressionTransformer{T}" /> must have a default constructor. To choose a transformer that does not
    ///     have a default constructor,
    ///     create your own custom attribute class implementing
    ///     <see cref="AttributeEvaluatingExpressionTransformer.IMethodCallExpressionTransformerAttribute" />.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    internal class MethodCallExpressionTransformerAttribute : Attribute,
        AttributeEvaluatingExpressionTransformer.IMethodCallExpressionTransformerAttribute
    {
        public MethodCallExpressionTransformerAttribute(Type transformerType)
        {
            ArgumentUtility.CheckNotNull("transformerType", transformerType);
            ArgumentUtility.CheckTypeIsAssignableFrom("transformerType", transformerType,
                typeof(IExpressionTransformer<MethodCallExpression>));

            TransformerType = transformerType;
        }

        public Type TransformerType { get; }

        public IExpressionTransformer<MethodCallExpression> GetExpressionTransformer(MethodCallExpression expression)
        {
            ArgumentUtility.CheckNotNull("expression", expression);
            try
            {
                return (IExpressionTransformer<MethodCallExpression>)Activator.CreateInstance(TransformerType);
            }
            catch (MissingMemberException ex)
            {
                var message = string.Format(
                    "The method call transformer '{0}' has no public default constructor and therefore cannot be used with the MethodCallExpressionTransformerAttribute.",
                    TransformerType);
                throw new InvalidOperationException(message, ex);
            }
        }
    }
}