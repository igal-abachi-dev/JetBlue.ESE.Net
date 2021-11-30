
using Microsoft.Isam.Esent.Interop;
using JetBlue.ESE.Net.Documents;
using JetBlue.ESE.Net.Serialization;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JetBlue.ESE.Net.Storage.Esent.Documents
{
    internal sealed class DocumentStore : DocumentSession
    {
        public const string KeyValueTableName = "Documents";
        public const string KeyColumnName = "DocumentId";
        public const int MaxKeyLen = 140;
        public const string KeyIndexName = "DocumentIds";
        public const string ValueColumnName = "Document";
        private readonly EsentDocumentSession _session;
        private readonly Table _documents;
        private readonly JET_COLUMNID _keyCol;
        private readonly JET_COLUMNID _valueCol;
        private readonly ILogger _log;

        public DocumentStore(EsentDocumentSession session, ILogger log)
        {
            this._session = session ?? throw new ArgumentNullException(nameof(session));
            this._log = log.ForContext<DocumentStore>();
            IDictionary<string, JET_COLUMNID> columns;
            this._session.OpenTable("Documents", out this._documents, out columns);
            this._keyCol = columns["DocumentId"];
            this._valueCol = columns["Document"];
        }

        public override bool TryLoad<T>(string id, out T document)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            Api.JetSetCurrentIndex(this._session.SessionId, (JET_TABLEID)this._documents, "DocumentIds");
            Api.MakeKey(this._session.SessionId, (JET_TABLEID)this._documents, id, Encoding.Unicode, MakeKeyGrbit.NewKey);
            if (!Api.TrySeek(this._session.SessionId, (JET_TABLEID)this._documents, SeekGrbit.SeekEQ))
            {
                document = default(T);
                return false;
            }
            string text = Api.RetrieveColumnAsString(this._session.SessionId, (JET_TABLEID)this._documents, this._valueCol, Encoding.Unicode);
            document = JetBlueSerializer.Deserialize<T>(text);
            return true;
        }

        public override IEnumerable<KeyValuePair<string, T>> Enumerate<T>(
          string idPrefix = null)
        {
            string prefix = idPrefix ?? JetBlueDocument.GetIdPrefix<T>();
            return (IEnumerable<KeyValuePair<string, T>>)this.GetValuesInternal<T>(prefix, (Func<string, bool>)(k => k.StartsWith(prefix))).ToArray<KeyValuePair<string, T>>();
        }

        private IEnumerable<KeyValuePair<string, T>> GetValuesInternal<T>(
          string keyPrefix,
          Func<string, bool> isIncluded)
        {
            Api.JetSetCurrentIndex(this._session.SessionId, (JET_TABLEID)this._documents, "DocumentIds");
            Api.MakeKey(this._session.SessionId, (JET_TABLEID)this._documents, keyPrefix, Encoding.Unicode, MakeKeyGrbit.NewKey);
            if (Api.TrySeek(this._session.SessionId, (JET_TABLEID)this._documents, SeekGrbit.SeekGE))
            {
                do
                {
                    string key = Api.RetrieveColumnAsString(this._session.SessionId, (JET_TABLEID)this._documents, this._keyCol, Encoding.Unicode);
                    if (!isIncluded(key))
                        break;
                    T obj = JetBlueSerializer.Deserialize<T>(Api.RetrieveColumnAsString(this._session.SessionId, (JET_TABLEID)this._documents, this._valueCol, Encoding.Unicode));
                    yield return new KeyValuePair<string, T>(key, obj);
                }
                while (Api.TryMoveNext(this._session.SessionId, (JET_TABLEID)this._documents));
            }
        }

        public override bool Remove(string id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            Api.JetSetCurrentIndex(this._session.SessionId, (JET_TABLEID)this._documents, "DocumentIds");
            Api.MakeKey(this._session.SessionId, (JET_TABLEID)this._documents, id, Encoding.Unicode, MakeKeyGrbit.NewKey);
            if (!Api.TrySeek(this._session.SessionId, (JET_TABLEID)this._documents, SeekGrbit.SeekEQ))
                return false;
            Api.JetDelete(this._session.SessionId, (JET_TABLEID)this._documents);
            return true;
        }

        public override void Store(string id, object document)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            string json = JetBlueSerializer.Serialize(document);
            this.Import(id, json);
        }

        private DocumentTransaction BeginTransaction()
        {
            Transaction txn = this._session.BeginTransaction();
            return new DocumentTransaction((Action)(() => txn.Commit(CommitTransactionGrbit.None)), new Action(((EsentResource)txn).Dispose));
        }

        public override void Dispose()
        {
            this._log.Verbose("Disposing document store");
            if (this._documents == null)
                return;
            try
            {
                this._documents.Dispose();
            }
            catch (EsentInvalidTableIdException ex)
            {
                this._log.Debug((Exception)ex, "Could not dispose the documents table; a transaction was probably rolled back, invalidating the table id");
            }
        }

        public override void Import(string id, string json)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            if (json == null)
                throw new ArgumentNullException(nameof(json));
            Api.JetSetCurrentIndex(this._session.SessionId, (JET_TABLEID)this._documents, "DocumentIds");
            Api.MakeKey(this._session.SessionId, (JET_TABLEID)this._documents, id, Encoding.Unicode, MakeKeyGrbit.NewKey);
            if (Api.TrySeek(this._session.SessionId, (JET_TABLEID)this._documents, SeekGrbit.SeekEQ))
            {
                using (DocumentTransaction documentTransaction = this.BeginTransaction())
                {
                    using (Update update = this._session.PrepareReplaceNoLock(this._documents))
                    {
                        Api.SetColumn(this._session.SessionId, (JET_TABLEID)this._documents, this._valueCol, json, Encoding.Unicode);
                        update.Save();
                        documentTransaction.Commit();
                    }
                }
            }
            else
            {
                using (DocumentTransaction documentTransaction = this.BeginTransaction())
                {
                    using (Update update = this._session.PrepareInsert(this._documents))
                    {
                        Api.SetColumn(this._session.SessionId, (JET_TABLEID)this._documents, this._keyCol, id, Encoding.Unicode);
                        Api.SetColumn(this._session.SessionId, (JET_TABLEID)this._documents, this._valueCol, json, Encoding.Unicode);
                        update.Save();
                        documentTransaction.Commit();
                    }
                }
            }
        }

        public override IEnumerable<KeyValuePair<string, string>> ExportAll<T>()
        {
            string keyPrefix = JetBlueDocument.GetIdPrefix<T>();
            Api.JetSetCurrentIndex(this._session.SessionId, (JET_TABLEID)this._documents, "DocumentIds");
            Api.MakeKey(this._session.SessionId, (JET_TABLEID)this._documents, keyPrefix, Encoding.Unicode, MakeKeyGrbit.NewKey);
            if (Api.TrySeek(this._session.SessionId, (JET_TABLEID)this._documents, SeekGrbit.SeekGE))
            {
                do
                {
                    string key = Api.RetrieveColumnAsString(this._session.SessionId, (JET_TABLEID)this._documents, this._keyCol, Encoding.Unicode);
                    if (!key.StartsWith(keyPrefix))
                        break;
                    string str = Api.RetrieveColumnAsString(this._session.SessionId, (JET_TABLEID)this._documents, this._valueCol, Encoding.Unicode);
                    yield return new KeyValuePair<string, string>(key, str);
                }
                while (Api.TryMoveNext(this._session.SessionId, (JET_TABLEID)this._documents));
            }
        }
    }
}
