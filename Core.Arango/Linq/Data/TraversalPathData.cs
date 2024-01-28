using Core.Arango.Linq.Attributes;
using Core.Arango.Linq.Interface;
using System.Collections.Generic;

namespace Core.Arango.Linq.Data
{
    [CollectionProperty(Naming = NamingConvention.ToCamelCase)]
    public class TraversalPathData<TVertex, TEdge>
    {
        public IList<TVertex> Vertices { get; set; }

        public IList<TEdge> Edges { get; set; }
    }
}