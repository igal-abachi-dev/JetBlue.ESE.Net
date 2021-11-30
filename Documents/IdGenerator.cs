
namespace JetBlue.ESE.Net.Documents
{
    public class IdGenerator
    {
        private readonly object _lock = new object();
        private const long BlockSize = 32;
        private IdSequence _sequence;
        private long? _next;

        public long Next(DocumentSession kvs)
        {
            lock (this._lock)
            {
                if (this._next.HasValue && this._next.Value < this._sequence.NextBlockStart)
                {
                    long num1 = this._next.Value;
                    long? next = this._next;
                    long num2 = 1;
                    this._next = next.HasValue ? new long?(next.GetValueOrDefault() + num2) : new long?();
                    return num1;
                }
                if (this._sequence == null && !kvs.TryLoad<IdSequence>("idsequence-global", out this._sequence))
                    this._sequence = new IdSequence()
                    {
                        NextBlockStart = 1L
                    };
                this._next = new long?(this._sequence.NextBlockStart);
                this._sequence.NextBlockStart += 32L;
                kvs.Store("idsequence-global", (object)this._sequence);
                return this.Next(kvs);
            }
        }
    }
}
