
using System;

namespace JetBlue.ESE.Net.Migration
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MigrationAttribute : Attribute
    {
        public MigrationAttribute(int order, string name)
        {
            this.Order = order;
            this.Name = name;
        }

        public int Order { get; }

        public string Name { get; }

        public bool IsVersionSpecific { get; set; }

        public bool SuppressTransaction { get; set; }
    }
}
