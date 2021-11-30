using JetBlue.ESE.Net.Documents;
using System;
using System.Collections.Generic;
using System.Linq;

namespace JetBlue.ESE.Net
{
    public abstract class DocumentSession : IDisposable
    {
        private static readonly IdGenerator IdGenerator = new IdGenerator();

        public abstract void Store(string id, object document);

        public abstract bool TryLoad<TDocument>(string id, out TDocument document);

        public abstract IEnumerable<KeyValuePair<string, TDocument>> Enumerate<TDocument>(
          string idPrefix = null);

        public abstract bool Remove(string id);

        public abstract IEnumerable<KeyValuePair<string, string>> ExportAll<TDocument>();

        public abstract void Import(string id, string json);

        public abstract void Dispose();

        public TDocument Load<TDocument>(string id)
        {
            TDocument document;
            if (!this.TryLoad<TDocument>(id, out document))
                throw new NotFoundException(id);
            return document;
        }

        public T[] LoadAll<T>() => this.Enumerate<T>().Select<KeyValuePair<string, T>, T>((Func<KeyValuePair<string, T>, T>)(kv => kv.Value)).ToArray<T>();

        public string NewId<TDocument>() => JetBlueDocument.GetIdPrefix<TDocument>() + DocumentSession.IdGenerator.Next(this).ToString();
    }
}
