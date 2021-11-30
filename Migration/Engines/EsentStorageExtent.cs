
using JetBlue.ESE.Net.Events;

namespace JetBlue.ESE.Net.Migration.Engines.Esent
{
    internal class EsentStorageExtent : StorageExtent
    {
        private readonly EsentDB _db;

        static EsentStorageExtent()
        {
            if (!EsentDB.EnableSparseFileSupport)
                return;
            Log.Information("ESENT sparse file support is available on this system");
        }

        public EsentStorageExtent(ExtentRange range, string dataFilePath, string instanceName = null)
          : base(range, dataFilePath)
        {
            this._db = new EsentDB(this.DataFilePath, instanceName ?? "Ese-Extent-" + this.Range.ToString());
            new EventStoreMigrator().Migrate(this._db);
        }

        public override IEnumerable<StructuredEvent> Get(
          CancellationToken cancel)
        {
            using (EsentDBSession session = this._db.BeginSession(tag: "event query"))
            {
                using (EventsTable es = new EventsTable(session))
                {
                    foreach (StructuredEvent structuredEvent in es.Get(cancel))
                        yield return structuredEvent;
                }
            }
        }

        public override void Add(StructuredEvent[] events)
        {
            using (EsentDBSession session = this._db.BeginSession(tag: "event write"))
            {
                using (EventsTable eventsTable = new EventsTable(session))
                    eventsTable.Add(events);
            }
        }

        public string Directory => Path.GetDirectoryName(this.DataFilePath);

        public override void Dispose() => this._db.Dispose();

        public override string ToString() => this.Range.ToString();
    }
}