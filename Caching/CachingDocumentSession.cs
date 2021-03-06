using JetBlue.ESE.Net.Documents;
using JetBlue.ESE.Net.Serialization;
using JetBlue.ESE.Net.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace JetBlue.ESE.Net.Caching
{
    public class CachingDocumentSession : DocumentSession
    {
        private readonly Recyclable<DocumentSession> _inner;
        private readonly UnboundedDocumentCache _cache;
        private readonly CancellationToken _disableWrites;

        public CachingDocumentSession(
          DocumentStore store,
          UnboundedDocumentCache cache,
          CancellationToken disableWrites)
        {
            this._inner = store != null ? new Recyclable<DocumentSession>(new Func<DocumentSession>(store.BeginSession)) : throw new ArgumentNullException(nameof(store));
            this._cache = cache ?? throw new ArgumentNullException(nameof(cache));
            this._disableWrites = disableWrites;
        }
        public override void Dispose() => this._inner.Dispose();

        private void EnsureWriteable()
        {
            if (this._disableWrites.IsCancellationRequested)
                throw new InvalidOperationException("The metadata store is read-only.");
        }

        public override void Store(string id, object document)
        {
            this.EnsureWriteable();
            string str = JetBlueSerializer.Serialize(document);
            this._cache.BeginWrite();
            try
            {
                Retry.WithRecovery(3, (Action)(() => this._inner.Value.Store(id, document)), (Action)(() => this._inner.Recycle()));
                this._cache.Set(id, str);
            }
            finally
            {
                this._cache.EndWrite();
            }
        }

        public override bool TryLoad<TDocument>(string id, out TDocument document)
        {
            string text;
            if (this._cache.TryGet(id, out text))
            {
                document = JetBlueSerializer.Deserialize<TDocument>(text);
                return true;
            }
            document = default(TDocument);
            return false;
        }

        public override IEnumerable<KeyValuePair<string, TDocument>> Enumerate<TDocument>(
          string idPrefix = null)
        {
            string docPrefix = idPrefix ?? JetBlueDocument.GetIdPrefix<TDocument>();
            return this._cache.Enumerate().Where<KeyValuePair<string, string>>((Func<KeyValuePair<string, string>, bool>)(kvp => docPrefix == null || kvp.Key.StartsWith(docPrefix))).Select<KeyValuePair<string, string>, KeyValuePair<string, TDocument>>((Func<KeyValuePair<string, string>, KeyValuePair<string, TDocument>>)(kvp => new KeyValuePair<string, TDocument>(kvp.Key, JetBlueSerializer.Deserialize<TDocument>(kvp.Value))));
        }

        public override bool Remove(string id)
        {
            this.EnsureWriteable();
            this._cache.BeginWrite();
            try
            {
                if (!Retry.WithRecovery<bool>(3, (Func<bool>)(() => this._inner.Value.Remove(id)), (Action)(() => this._inner.Recycle())))
                    return false;
                this._cache.Unset(id);
                return true;
            }
            finally
            {
                this._cache.EndWrite();
            }
        }

        public override IEnumerable<KeyValuePair<string, string>> ExportAll<T>()
        {
            string docPrefix = JetBlueDocument.GetIdPrefix<T>();
            return this._cache.Enumerate().Where<KeyValuePair<string, string>>((Func<KeyValuePair<string, string>, bool>)(kvp => docPrefix == null || kvp.Key.StartsWith(docPrefix)));
        }

        public override void Import(string id, string json)
        {
            this.EnsureWriteable();
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            this._cache.BeginWrite();
            try
            {
                Retry.WithRecovery(3, (Action)(() => this._inner.Value.Import(id, json)), (Action)(() => this._inner.Recycle()));
                this._cache.Set(id, json);
            }
            finally
            {
                this._cache.EndWrite();
            }
        }
    }
}
