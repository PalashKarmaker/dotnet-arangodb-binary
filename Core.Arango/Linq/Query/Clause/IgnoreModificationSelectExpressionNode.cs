﻿using Core.Arango.Relinq;
using Core.Arango.Relinq.Parsing.Structure.IntermediateModel;
using System.Linq.Expressions;
using System.Reflection;

namespace Core.Arango.Linq.Query.Clause
{
    internal class IgnoreModificationSelectExpressionNode : MethodCallExpressionNodeBase
    {
        public static readonly MethodInfo[] SupportedMethods =
        {
            LinqUtility.GetSupportedMethod(() => ArangoQueryableExtensions.IgnoreModificationSelect<object>(null))
        };

        public IgnoreModificationSelectExpressionNode(MethodCallExpressionParseInfo parseInfo)
            : base(parseInfo)
        {
        }

        public override Expression Resolve(ParameterExpression inputParameter, Expression expressionToBeResolved,
            ClauseGenerationContext clauseGenerationContext)
        {
            LinqUtility.CheckNotNull("inputParameter", inputParameter);
            LinqUtility.CheckNotNull("expressionToBeResolved", expressionToBeResolved);

            return Source.Resolve(inputParameter, expressionToBeResolved, clauseGenerationContext);
        }

        protected override void ApplyNodeSpecificSemantics(QueryModel queryModel,
            ClauseGenerationContext clauseGenerationContext)
        {
            LinqUtility.CheckNotNull("queryModel", queryModel);

            var modificationClause = queryModel.BodyClauses.NextBodyClause<IModificationClause>();
            modificationClause.IgnoreSelect = true;
        }
    }
}