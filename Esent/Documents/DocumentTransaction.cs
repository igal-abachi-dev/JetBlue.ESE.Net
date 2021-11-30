
using System;

namespace JetBlue.ESE.Net.Storage.Esent.Documents
{
    internal sealed class DocumentTransaction : IDisposable
    {
        private readonly Action _onCommit;
        private readonly Action _onDispose;

        public DocumentTransaction(Action onCommit, Action onDispose)
        {
            this._onCommit = onCommit;
            this._onDispose = onDispose;
        }

        public void Commit() => this._onCommit();

        public void Dispose() => this._onDispose();
    }
}
