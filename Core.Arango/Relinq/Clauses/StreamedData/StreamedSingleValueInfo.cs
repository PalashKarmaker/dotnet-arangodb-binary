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

using Core.Arango.Relinq.Clauses.ResultOperators;
using Core.Arango.Relinq.Utilities;
using Remotion.Utilities;
using System;
using System.Reflection;

namespace Core.Arango.Relinq.Clauses.StreamedData
{
    /// <summary>
    ///     Describes a single value streamed out of a <see cref="QueryModel" /> or <see cref="ResultOperatorBase" />. A single
    ///     value corresponds to one
    ///     item from the result set, as produced by <see cref="FirstResultOperator" /> or <see cref="SingleResultOperator" />,
    ///     for instance.
    /// </summary>
    internal sealed class StreamedSingleValueInfo : StreamedValueInfo
    {
        private static readonly MethodInfo s_executeMethod =
            typeof(StreamedSingleValueInfo).GetRuntimeMethodChecked("ExecuteSingleQueryModel",
                new[] { typeof(QueryModel), typeof(IQueryExecutor) });

        public StreamedSingleValueInfo(Type dataType, bool returnDefaultWhenEmpty)
            : base(dataType)
        {
            ReturnDefaultWhenEmpty = returnDefaultWhenEmpty;
        }

        public bool ReturnDefaultWhenEmpty { get; }

        public override IStreamedData ExecuteQueryModel(QueryModel queryModel, IQueryExecutor executor)
        {
            ArgumentUtility.CheckNotNull("queryModel", queryModel);
            ArgumentUtility.CheckNotNull("executor", executor);

            var executeMethod = s_executeMethod.MakeGenericMethod(DataType);
            // wrap executeMethod into a delegate instead of calling Invoke in order to allow for exceptions that are bubbled up correctly
            var func = (Func<QueryModel, IQueryExecutor, object>)executeMethod.CreateDelegate(
                typeof(Func<QueryModel, IQueryExecutor, object>), this);
            var result = func(queryModel, executor);

            return new StreamedValue(result, this);
        }

        protected override StreamedValueInfo CloneWithNewDataType(Type dataType)
        {
            return new StreamedSingleValueInfo(dataType, ReturnDefaultWhenEmpty);
        }

        public object ExecuteSingleQueryModel<T>(QueryModel queryModel, IQueryExecutor executor)
        {
            ArgumentUtility.CheckNotNull("queryModel", queryModel);
            ArgumentUtility.CheckNotNull("executor", executor);

            return executor.ExecuteSingle<T>(queryModel, ReturnDefaultWhenEmpty);
        }

        public override bool Equals(IStreamedDataInfo obj)
        {
            return base.Equals(obj) && ((StreamedSingleValueInfo)obj).ReturnDefaultWhenEmpty == ReturnDefaultWhenEmpty;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ ReturnDefaultWhenEmpty.GetHashCode();
        }
    }
}