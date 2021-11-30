
using Microsoft.Isam.Esent.Interop;
using JetBlue.ESE.Net.Migration;
using Serilog;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JetBlue.ESE.Net.Storage.Esent.Migration
{
    internal static class Migrator
    {
        private const string SchemaVersionsTableName = "SchemaVersions";
        private const string IdColumnName = "Id";
        private const string IdIndexName = "IdIndex";
        private const string MigrationNameColumnName = "MigrationName";
        private const int MigrationNameColumnLength = 127;
        private const string AppliedAtUtcColumnName = "AppliedAtUtc";

        public static void Migrate(
          EsentDocumentStore store,
          IEnumerable<Lazy<IMigration, MigrationMetadata>> migrations,
          ILogger migrationLog)
        {
            ILogger log = migrationLog.ForContext("SourceContext", (object)typeof(Migrator).FullName, false);
            log.Debug("Checking database schema version");
            MigrationState applied = Migrator.GetAppliedMigrations(store, log);
            bool firstRun = applied.AppliedMigrations.Count == 0;
            if (firstRun)
                log.Information("Initializing a new metastore");
            var array = migrations.Select(m => new
            {
                m = m,
                name = Migrator.GetMigrationName(m.Metadata)
            }).Where(_param1 => !applied.AppliedMigrations.ContainsKey(_param1.name)).OrderBy(_param1 => _param1.m.Metadata.Order).Select(_param1 => new
            {
                Metadata = _param1.m.Metadata,
                Value = _param1.m.Value,
                Name = _param1.name
            }).ToArray();
            log.Debug<int>("{CountToApply} migrations to apply", array.Length);
            using (EsentDocumentSession session = store.BeginEsentDocumentSession("migration"))
            {
                Table table;
                IDictionary<string, JET_COLUMNID> columns;
                session.OpenTable("SchemaVersions", out table, out columns);
                JET_COLUMNID migrationNameColumn = columns["MigrationName"];
                JET_COLUMNID appliedAtColumn = columns["AppliedAtUtc"];
                foreach (var data in array)
                    Migrator.Apply(session, data.Name, !data.Metadata.SuppressTransaction, data.Value, table, migrationNameColumn, appliedAtColumn, log, firstRun);
            }
            log.Debug("All migrations complete");
        }

        private static string GetMigrationName(MigrationMetadata migration)
        {
            string str = migration.Name;
            if (migration.IsVersionSpecific)
                str = str + " - " + migration.Type.Assembly.GetName().Version?.ToString();
            return str;
        }

        private static void Apply(
          EsentDocumentSession session,
          string migrationName,
          bool transactional,
          IMigration migration,
          Table schemaVersions,
          JET_COLUMNID migrationNameColumn,
          JET_COLUMNID appliedAtColumn,
          ILogger log,
          bool firstRun)
        {
            LogEventLevel logEventLevel = firstRun ? (LogEventLevel)1 : (LogEventLevel)2;
            log.Write<string>(logEventLevel, "Applying migration {MigrationName}", migrationName);
            Transaction transaction = (Transaction)null;
            if (transactional)
                transaction = session.BeginTransaction();
            else
                log.Write<string>(logEventLevel, "Transaction suppressed for {MigrationName}", migrationName);
            try
            {
                migration.Apply((DocumentSession)session);
                using (Update update = session.PrepareInsert(schemaVersions))
                {
                    Api.SetColumn(session.SessionId, (JET_TABLEID)schemaVersions, migrationNameColumn, migrationName, Encoding.Unicode);
                    Api.SetColumn(session.SessionId, (JET_TABLEID)schemaVersions, appliedAtColumn, DateTime.UtcNow);
                    update.Save();
                }
                transaction?.Commit(CommitTransactionGrbit.None);
                log.Debug("Migration applied successfully");
            }
            finally
            {
                transaction?.Dispose();
            }
        }

        public static MigrationState GetAppliedMigrations(
          EsentDocumentStore store,
          ILogger log)
        {
            MigrationState migrationState = new MigrationState();
            DateTime utcNow = DateTime.UtcNow;
            using (EsentDocumentSession session = store.BeginEsentDocumentSession("migration check"))
            {
                Table table;
                IDictionary<string, JET_COLUMNID> columns;
                try
                {
                    session.OpenTable("SchemaVersions", out table, out columns);
                }
                catch (EsentObjectNotFoundException ex)
                {
                    Migrator.CreateSchemaVersionsTable(session, log);
                    return migrationState;
                }
                JET_COLUMNID columnid = columns["MigrationName"];
                Api.JetSetCurrentIndex(session.SessionId, (JET_TABLEID)table, "IdIndex");
                if (!Api.TryMoveFirst(session.SessionId, (JET_TABLEID)table))
                    return migrationState;
                do
                {
                    string key = Api.RetrieveColumnAsString(session.SessionId, (JET_TABLEID)table, columnid, Encoding.Unicode);
                    migrationState.AppliedMigrations.Add(key, utcNow);
                }
                while (Api.TryMoveNext(session.SessionId, (JET_TABLEID)table));
                return migrationState;
            }
        }

        private static void CreateSchemaVersionsTable(EsentDocumentSession session, ILogger log)
        {
            log.Information<string>("Creating {SchemaVersions} table", "SchemaVersions");
            using (Transaction transaction = new Transaction(session.SessionId))
            {
                JET_TABLEID tableid1;
                Api.JetCreateTable(session.SessionId, session.DatabaseId, "SchemaVersions", 0, 100, out tableid1);

                JET_SESID sessionId1 = session.SessionId;
                JET_TABLEID tableid2 = tableid1;
                JET_COLUMNDEF columndef1 = new JET_COLUMNDEF();
                columndef1.coltyp = JET_coltyp.Long;
                columndef1.grbit = ColumndefGrbit.ColumnFixed | ColumndefGrbit.ColumnNotNULL | ColumndefGrbit.ColumnAutoincrement;
                JET_COLUMNID jetColumnid;
                Api.JetAddColumn(sessionId1, tableid2, "Id", columndef1, (byte[])null, 0, out jetColumnid);

                JET_SESID sessionId2 = session.SessionId;
                JET_TABLEID tableid3 = tableid1;
                JET_COLUMNDEF columndef2 = new JET_COLUMNDEF();
                columndef2.coltyp = JET_coltyp.Text;
                columndef2.grbit = ColumndefGrbit.ColumnNotNULL;
                columndef2.cbMax = 254;
                columndef2.cp = JET_CP.Unicode;
                ref JET_COLUMNID local2 = ref jetColumnid;
                Api.JetAddColumn(sessionId2, tableid3, "MigrationName", columndef2, (byte[])null, 0, out local2);

                JET_SESID sessionId3 = session.SessionId;
                JET_TABLEID tableid4 = tableid1;
                JET_COLUMNDEF columndef3 = new JET_COLUMNDEF();
                columndef3.coltyp = JET_coltyp.DateTime;
                columndef3.grbit = ColumndefGrbit.ColumnFixed | ColumndefGrbit.ColumnNotNULL;
                ref JET_COLUMNID local3 = ref jetColumnid;
                Api.JetAddColumn(sessionId3, tableid4, "AppliedAtUtc", columndef3, (byte[])null, 0, out local3);

                Api.JetCreateIndex(session.SessionId, tableid1, "IdIndex", CreateIndexGrbit.IndexUnique | CreateIndexGrbit.IndexPrimary, "+Id\0\0", "+Id\0\0".Length, 100);
                transaction.Commit(CommitTransactionGrbit.None);
            }
        }
    }
}
