
using System;

namespace JetBlue.ESE.Net.Migration
{
    public class MigrationMetadata
    {
        public int Order { get; set; }

        public string Name { get; set; }

        public bool IsVersionSpecific { get; set; }

        public bool SuppressTransaction { get; set; }

        public Type Type { get; set; }
    }
}
