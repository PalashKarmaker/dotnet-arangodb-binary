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

using Core.Arango.Relinq.Clauses.StreamedData;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Core.Arango.Relinq.Clauses.ResultOperators
{
    /// <summary>
    ///     Represents taking the single item returned by a query.
    ///     This is a result operator, operating on the whole result set of a query.
    /// </summary>
    /// <example>
    ///     In C#, the "Single" call in the following example corresponds to a <see cref="SingleResultOperator" />.
    ///     <code>
    /// var query = (from s in Students
    ///              select s).Single();
    /// </code>
    /// </example>
    internal sealed class SingleResultOperator : ChoiceResultOperatorBase
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="SingleResultOperator" />.
        /// </summary>
        /// <param name="returnDefaultWhenEmpty">The flag defines if a default expression should be regarded.</param>
        public SingleResultOperator(bool returnDefaultWhenEmpty)
            : base(returnDefaultWhenEmpty)
        {
        }

        public override ResultOperatorBase Clone(CloneContext cloneContext)
        {
            return new SingleResultOperator(ReturnDefaultWhenEmpty);
        }

        public override StreamedValue ExecuteInMemory<T>(StreamedSequence input)
        {
            var sequence = input.GetTypedSequence<T>();
            var result = ReturnDefaultWhenEmpty ? sequence.SingleOrDefault() : sequence.Single();
            return new StreamedValue(result, GetOutputDataInfo(input.DataInfo));
        }

        /// <inheritdoc />
        public override void TransformExpressions(Func<Expression, Expression> transformation)
        {
            //nothing to do here
        }

        public override string ToString()
        {
            if (ReturnDefaultWhenEmpty)
                return "SingleOrDefault()";
            return "Single()";
        }
    }
}