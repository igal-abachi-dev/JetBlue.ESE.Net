
using System;


#nullable enable
namespace JetBlue.ESE.Net.Util
{
    public sealed class Recyclable<T> : IDisposable where T : class, IDisposable
    {
        private readonly Func<T> _createValue;
        private T? _value;

        public Recyclable(Func<T> createValue) => this._createValue = createValue ?? throw new ArgumentNullException(nameof(createValue));

        public bool IsValueCreated => (object)this._value != null;

        public T Value => this._value ?? (this._value = this._createValue());

        public void Recycle()
        {
            if ((object)this._value == null)
                return;
            T obj = this._value;
            this._value = default(T);
            obj.Dispose();
        }

        public void Dispose() => this.Recycle();
    }
}
