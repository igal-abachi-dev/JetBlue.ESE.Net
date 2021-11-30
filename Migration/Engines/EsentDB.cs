using Microsoft.Isam.Esent.Interop;
using Microsoft.Isam.Esent.Interop.Windows81;
using Serilog;

namespace JetBlue.ESE.Net.Migration.Engines.Esent.Data
{
    internal sealed class EsentDB : IDisposable
    {
        private const string DefaultInstanceName = "JetBlue";
        private readonly string _path;
        private readonly Instance _instance;
        private readonly ILogger _log = Log.ForContext<EsentDB>();

        public EsentDB(string path, string instanceName = "JetBlue")
        {
            this._path = path != null ? Path.GetFullPath(path) : throw new ArgumentNullException(nameof(path));
            this._log.Debug<string, string>("Opening event store extent {DatabasePath} as {InstanceName}", this._path, instanceName);
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
                    string propertyValue = directoryName;
                    Log.Fatal<string>(ex, "Unable to create directory '{Directory:l}'", propertyValue);
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
            if (EsentDB.EnableSparseFileSupport)
                this._instance.Parameters.EnableShrinkDatabase = ShrinkDatabaseGrbit.On | ShrinkDatabaseGrbit.Realtime;
            this._instance.Init();
            if (File.Exists(this._path))
                return;
            this._log.Information<string>("Creating new data file at {DatabasePath}", this._path);
            using (Session session = new Session((JET_INSTANCE)this._instance))
                Api.JetCreateDatabase((JET_SESID)session, this._path, (string)null, out JET_DBID _, (CreateDatabaseGrbit)2048);
        }

        public EsentDBSession BeginSession(Action notifyDisposed = null, string tag = null)
        {
            this._log.Verbose<string>("Beginning a new {Tag} session", tag);
            return new EsentDBSession(this._instance, this._path, notifyDisposed);
        }

        public void Dispose() => this._instance?.Dispose();

        public static bool EnableSparseFileSupport => Environment.OSVersion.Version >= new Version(6, 2, 9600, 0);
    }
}
