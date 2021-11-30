
using JetBlue.ESE.Net.Migration;
using System;
using System.Collections.Generic;

namespace JetBlue.ESE.Net
{
    public abstract class DocumentStore : IDisposable
    {
        public abstract void Migrate(
          IEnumerable<Lazy<IMigration, MigrationMetadata>> migrations);

        public abstract MigrationState ExportMigrationState();

        public abstract void ImportMigrationState(MigrationState migrationState);

        public abstract DocumentSession BeginSession(string tag = null);

        public abstract void Dispose();
    }
}
