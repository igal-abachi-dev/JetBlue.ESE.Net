
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

namespace JetBlue.ESE.Net.Caching
{
    public class UnboundedDocumentCache
    {
        private readonly object _sync = new object();
        private volatile ImmutableSortedDictionary<string, string> _current = (ImmutableSortedDictionary<string, string>)ImmutableSortedDictionary<string, string>.Empty;

        public void BeginWrite() => Monitor.Enter(this._sync);

        public void EndWrite() => Monitor.Exit(this._sync);

        public void Set(string key, string value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (value == null)
                throw new ArgumentNullException(nameof(value));
            this._current = this._current.SetItem(key, value);
        }

        public void Unset(string key) => this._current = this._current.Remove(key);

        public void Clear() => this._current = (ImmutableSortedDictionary<string, string>)ImmutableSortedDictionary<string, string>.Empty;

        public bool TryGet(string key, out string value) => this._current.TryGetValue(key, out value);

        public IEnumerable<KeyValuePair<string, string>> Enumerate() => (IEnumerable<KeyValuePair<string, string>>)this._current;
    }
}
