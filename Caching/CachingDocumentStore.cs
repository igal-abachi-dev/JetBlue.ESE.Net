
using JetBlue.ESE.Net.Migration;
using System;
using System.Collections.Generic;
using System.Threading;

namespace JetBlue.ESE.Net.Caching
{
    public class CachingDocumentStore : DocumentStore
    {
        private readonly DocumentStore _inner;
        private readonly UnboundedDocumentCache _cache = new UnboundedDocumentCache();

        public CachingDocumentStore(DocumentStore inner)
        {
            this._inner = inner ?? throw new ArgumentNullException(nameof(inner));
            using (DocumentSession documentSession = this._inner.BeginSession())
            {
                foreach (KeyValuePair<string, string> keyValuePair in documentSession.ExportAll<object>())
                    this._cache.Set(keyValuePair.Key, keyValuePair.Value);
            }
        }

        public override void Migrate(MigrationList migrations) => throw new InvalidOperationException("Document store migration must be completed before construction of the caching wrapper.");


        public override MigrationState ExportMigrationState() => this._inner.ExportMigrationState();

        public override bool CanImportMigrationState => this._inner.CanImportMigrationState;
        public override void ImportMigrationState(MigrationState migrationState) => this._inner.ImportMigrationState(migrationState);

        public override DocumentSession BeginSession() => (DocumentSession)new CachingDocumentSession(this._inner, this._cache, CancellationToken.None);

        public override void Dispose() => this._inner.Dispose();
    }
}
