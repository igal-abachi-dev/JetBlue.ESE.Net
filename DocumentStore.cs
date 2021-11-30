
using JetBlue.ESE.Net.Migration;
using System;

namespace JetBlue.ESE.Net
{
    public abstract class DocumentStore : IDisposable
    {
        public abstract void Migrate(MigrationList migrations);

        public abstract MigrationState ExportMigrationState();

        public virtual bool CanImportMigrationState => true;

        public abstract void ImportMigrationState(MigrationState migrationState);

        public abstract DocumentSession BeginSession();

        public abstract void Dispose();
    }
}
