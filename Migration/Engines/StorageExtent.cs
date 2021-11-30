
using JetBlue.ESE.Net.Events;

namespace JetBlue.ESE.Net.Migration.Engines
{
    public abstract class StorageExtent : IDisposable
    {
        public ExtentRange Range { get; }

        public string DataFilePath { get; }

        protected StorageExtent(ExtentRange range, string dataFilePath)
        {
            if (!Path.IsPathFullyQualified(dataFilePath))
                throw new ArgumentException("Data file path `" + dataFilePath + "` is not fully qualified.");
            this.Range = range;
            this.DataFilePath = dataFilePath ?? throw new ArgumentNullException(nameof(dataFilePath));
        }

        public abstract void Dispose();

        public abstract void Add(StructuredEvent[] events);

        public abstract IEnumerable<StructuredEvent> Get(
          CancellationToken cancel);
    }
}
