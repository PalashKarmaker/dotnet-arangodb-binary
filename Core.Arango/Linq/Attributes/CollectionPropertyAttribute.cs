using Core.Arango.Linq.Interface;
using System;

namespace Core.Arango.Linq.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CollectionPropertyAttribute : Attribute, ICollectionPropertySetting
    {
        public string CollectionName { get; set; }

        public NamingConvention Naming { get; set; }
    }
}