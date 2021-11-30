
using Microsoft.Isam.Esent.Interop;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading;

namespace JetBlue.ESE.Net.Storage.Esent
{
    public sealed class EsentDocumentSession : DocumentSession
    {
        private readonly JET_DBID _dbid;
        private readonly Session _session;
        private readonly ILogger _log;
        private readonly Lazy<JetBlue.ESE.Net.Storage.Esent.Documents.DocumentStore> _documents;
        private int _disposed;
        private static int _activeSessions;

        public static int ActiveSessions => EsentDocumentSession._activeSessions;

        internal EsentDocumentSession(Instance instance, string path)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            if (path == null)
                throw new ArgumentNullException(nameof(path));
            this._session = new Session((JET_INSTANCE)instance);
            Interlocked.Increment(ref EsentDocumentSession._activeSessions);
            try
            {
                this._log = Log.ForContext<EsentDocumentSession>().ForContext(nameof(SessionId), (object)this._session.JetSesid, false);
                int num1 = (int)Api.JetAttachDatabase((JET_SESID)this._session, path, AttachDatabaseGrbit.None);
                int num2 = (int)Api.JetOpenDatabase((JET_SESID)this._session, path, (string)null, out this._dbid, OpenDatabaseGrbit.None);
                this._documents = new Lazy<JetBlue.ESE.Net.Storage.Esent.Documents.DocumentStore>((Func<JetBlue.ESE.Net.Storage.Esent.Documents.DocumentStore>)(() => new JetBlue.ESE.Net.Storage.Esent.Documents.DocumentStore(this, this._log)));
            }
            catch (Exception ex)
            {
                this._log.Error(ex, "Session initialization failed");
                Interlocked.Decrement(ref EsentDocumentSession._activeSessions);
                this._session.Dispose();
                throw;
            }
        }

        internal JET_SESID SessionId => (JET_SESID)this._session;

        internal JET_DBID DatabaseId => this._dbid;

        internal void OpenTable(
          string tableName,
          out Table table,
          out IDictionary<string, JET_COLUMNID> columns)
        {
            table = new Table((JET_SESID)this._session, this._dbid, tableName, OpenTableGrbit.None);
            columns = Api.GetColumnDictionary((JET_SESID)this._session, (JET_TABLEID)table);
        }

        internal Transaction BeginTransaction() => new Transaction((JET_SESID)this._session);

        internal Update PrepareInsert(Table table) => new Update((JET_SESID)this._session, (JET_TABLEID)table, JET_prep.Insert);

        internal Update PrepareReplaceNoLock(Table table) => new Update((JET_SESID)this._session, (JET_TABLEID)table, JET_prep.ReplaceNoLock);

        public override void Store(string id, object document) => this._documents.Value.Store(id, document);

        public override bool TryLoad<TDocument>(string id, out TDocument document) => this._documents.Value.TryLoad<TDocument>(id, out document);

        public override IEnumerable<KeyValuePair<string, TDocument>> Enumerate<TDocument>(
          string idPrefix = null)
        {
            return this._documents.Value.Enumerate<TDocument>(idPrefix);
        }

        public override bool Remove(string id) => this._documents.Value.Remove(id);

        public override IEnumerable<KeyValuePair<string, string>> ExportAll<T>() => this._documents.Value.ExportAll<T>();

        public override void Import(string id, string json) => this._documents.Value.Import(id, json);

        public override void Dispose()
        {
            if (Interlocked.CompareExchange(ref this._disposed, 1, 0) == 1)
                return;
            this._log.Verbose("Disposing session");
            if (this._documents.IsValueCreated)
                this._documents.Value.Dispose();
            try
            {
                Interlocked.Decrement(ref EsentDocumentSession._activeSessions);
                this._session.Dispose();
            }
            catch (Exception ex)
            {
                this._log.Error(ex, "Session disposal failed");
                throw;
            }
        }
    }
}
