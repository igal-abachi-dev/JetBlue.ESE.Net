
using Microsoft.Isam.Esent.Interop;
using JetBlue.ESE.Net.Migration;
using JetBlue.ESE.Net.Storage.Esent.Migration;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;

namespace JetBlue.ESE.Net.Storage.Esent
{
    public sealed class EsentDocumentStore : DocumentStore
    {
        private const string DefaultInstanceName = "JetBlue";
        private readonly string _path;
        private readonly Instance _instance;
        private readonly ILogger _log;

        public EsentDocumentStore(string path, string instanceName = "JetBlue")
        {
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            this._log = instanceName != null ? Log.ForContext<EsentDocumentStore>().ForContext("InstanceName", (object)instanceName, false) : throw new ArgumentNullException(nameof(instanceName));
            this._path = Path.GetFullPath(path);
            this._log.Information<string>("Opening metastore {MetastorePath}", this._path);
            string directoryName = Path.GetDirectoryName(this._path);
            if (directoryName != null && !Directory.Exists(directoryName))
            {
                this._log.Debug<string>("{DatabaseDirectory} doesn't exist, creating", directoryName);
                try
                {
                    Directory.CreateDirectory(directoryName);
                }
                catch (Exception ex)
                {
                    string str = directoryName;
                    Log.Fatal<string>(ex, "Unable to create directory '{Directory:l}'", str);
                    Environment.Exit(1);
                }
            }
            this._instance = new Instance(instanceName);
            this._instance.Parameters.CircularLog = true;
            this._instance.Parameters.SystemDirectory = directoryName;
            this._instance.Parameters.LogFileDirectory = directoryName;
            this._instance.Parameters.AlternateDatabaseRecoveryDirectory = directoryName;
            this._instance.Parameters.TempDirectory = directoryName;
            this._instance.Parameters.MaxSessions = 1000;
            this._instance.Parameters.MaxVerPages = 256;
            this._instance.Parameters.MaxTemporaryTables = 1024;
            this._instance.Init();
            if (File.Exists(this._path))
                return;
            EsentDocumentStore.CreateDatabase(this._log, this._path, this._instance);
        }

        private static void CreateDatabase(ILogger log, string path, Instance instance)
        {
            log.Information<string>("Creating new data file at {MetastorePath}", path);
            using (Session session = new Session((JET_INSTANCE)instance))
            {
                JET_DBID dbid;
                Api.JetCreateDatabase((JET_SESID)session, path, (string)null, out dbid, CreateDatabaseGrbit.None);

                JET_TABLEID tableid1;
                Api.JetCreateTable((JET_SESID)session, dbid, "Documents", 0, 100, out tableid1);

                JET_SESID sesid1 = (JET_SESID)session;
                JET_TABLEID tableid2 = tableid1;
                JET_COLUMNDEF columndef1 = new JET_COLUMNDEF();
                columndef1.coltyp = JET_coltyp.Text;
                columndef1.cbMax = 140;
                columndef1.grbit = ColumndefGrbit.ColumnNotNULL;
                columndef1.cp = JET_CP.Unicode;
                JET_COLUMNID jetColumnid;
                Api.JetAddColumn(sesid1, tableid2, "DocumentId", columndef1, (byte[])null, 0, out jetColumnid);

                JET_SESID sesid2 = (JET_SESID)session;
                JET_TABLEID tableid3 = tableid1;
                JET_COLUMNDEF columndef2 = new JET_COLUMNDEF();
                columndef2.coltyp = JET_coltyp.LongText;
                columndef2.grbit = ColumndefGrbit.ColumnNotNULL;
                columndef2.cp = JET_CP.Unicode;
                ref JET_COLUMNID local2 = ref jetColumnid;
                Api.JetAddColumn(sesid2, tableid3, "Document", columndef2, (byte[])null, 0, out local2);

                Api.JetCreateIndex((JET_SESID)session, tableid1, "DocumentIds", CreateIndexGrbit.IndexPrimary, "+DocumentId\0\0", "+DocumentId\0\0".Length, 100);
            }
        }

        public override void Migrate(
          IEnumerable<Lazy<IMigration, MigrationMetadata>> migrations)
        {
            Migrator.Migrate(this, migrations, this._log);
        }

        internal EsentDocumentSession BeginEsentDocumentSession(string tag = null)
        {
            this._log.Verbose<string>("Beginning a new {Tag} document session", tag);
            return new EsentDocumentSession(this._instance, this._path);
        }

        public override DocumentSession BeginSession(string tag = null) => (DocumentSession)this.BeginEsentDocumentSession();

        public override void Dispose()
        {
            if (this._instance == null)
                return;
            this._log.Information("Closing metastore");
            this._instance.Dispose();
        }

        public override MigrationState ExportMigrationState() => Migrator.GetAppliedMigrations(this, this._log);

        public override void ImportMigrationState(MigrationState migrationState) => throw new NotSupportedException();
    }
}
