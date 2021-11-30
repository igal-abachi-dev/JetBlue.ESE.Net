
namespace JetBlue.ESE.Net.Events
{
    public class StructuredEvent : IEquatable<StructuredEvent>, IComparable<StructuredEvent>
    {
        private readonly Guid _key;
        private readonly uint _eventType;
        private readonly IReadOnlyDictionary<string, object> _properties;
        private readonly IReadOnlyDictionary<string, object> _renderings;
        private readonly string _exception;
        private readonly string _renderedMessage;
        private readonly string _messageTemplate;
        private readonly string _level;
        public static readonly StructuredEvent Empty = new StructuredEvent(Guid.Empty, 0U, (IReadOnlyDictionary<string, object>)SchematicProperties.Empty);

        public static StructuredEvent WithProperties(SchematicProperties properties) => new StructuredEvent(properties);

        private StructuredEvent(SchematicProperties properties)
        {
            this._key = StructuredEvent.Empty.Key;
            this._eventType = StructuredEvent.Empty.EventType;
            this._properties = (IReadOnlyDictionary<string, object>)(properties ?? throw new ArgumentNullException(nameof(properties)));
            this._renderings = StructuredEvent.Empty.Renderings;
            this._exception = StructuredEvent.Empty.Exception;
            this._renderedMessage = StructuredEvent.Empty.RenderedMessage;
            this._messageTemplate = StructuredEvent.Empty.MessageTemplate;
            this._level = StructuredEvent.Empty.Level;
        }

        public StructuredEvent(Guid key, uint eventType, IReadOnlyDictionary<string, object> document)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));
            this._key = key;
            this._eventType = eventType;
            object obj1;
            document.TryGetValue(nameof(Level), out obj1);
            this._level = obj1 as string;
            object obj2;
            document.TryGetValue(nameof(Exception), out obj2);
            this._exception = obj2 as string;
            object obj3;
            document.TryGetValue(nameof(RenderedMessage), out obj3);
            this._renderedMessage = obj3 as string;
            object obj4;
            document.TryGetValue(nameof(MessageTemplate), out obj4);
            this._messageTemplate = obj4 as string;
            object obj5;
            document.TryGetValue(nameof(Properties), out obj5);
            if (!(obj5 is IReadOnlyDictionary<string, object> readOnlyDictionary))
                readOnlyDictionary = (IReadOnlyDictionary<string, object>)SchematicProperties.Empty;
            this._properties = readOnlyDictionary;
            object obj6;
            document.TryGetValue(nameof(Renderings), out obj6);
            this._renderings = obj6 as IReadOnlyDictionary<string, object>;
        }

        public StructuredEvent(
          long arrivalOrder,
          DateTime utcTimestamp,
          uint eventType,
          string level,
          string exception,
          string renderedMessage,
          string messageTemplate,
          IReadOnlyDictionary<string, object> properties,
          IReadOnlyDictionary<string, object> renderings)
        {
            this._key = StorageEvent.MakeKey(arrivalOrder, utcTimestamp);
            this._eventType = eventType;
            this._level = level;
            this._exception = exception;
            this._renderedMessage = renderedMessage;
            this._messageTemplate = messageTemplate;
            this._properties = properties ?? (IReadOnlyDictionary<string, object>)SchematicProperties.Empty;
            this._renderings = renderings;
        }

        public Guid Key => this._key;

        public IReadOnlyDictionary<string, object> Properties => this._properties;

        public IReadOnlyDictionary<string, object> Renderings => this._renderings;

        public string Level => this._level;

        public string Exception => this._exception;

        public string RenderedMessage => this._renderedMessage;

        public string MessageTemplate => this._messageTemplate;

        public long ArrivalOrder
        {
            get
            {
                long arrivalOrder;
                StorageEvent.UnpackKey(this._key, out DateTime _, out arrivalOrder);
                return arrivalOrder;
            }
        }

        public DateTime UtcTimestamp
        {
            get
            {
                DateTime utcTimestamp;
                StorageEvent.UnpackKey(this._key, out utcTimestamp, out long _);
                return utcTimestamp;
            }
        }

        public uint EventType => this._eventType;

        public override bool Equals(object other) => other is StructuredEvent other1 && this.Equals(other1);

        public override int GetHashCode() => this._key.GetHashCode();

        public bool Equals(StructuredEvent other) => other != null && this._key.Equals(other.Key);

        public int CompareTo(StructuredEvent other)
        {
            if (this.UtcTimestamp < other.UtcTimestamp)
                return -1;
            if (this.UtcTimestamp > other.UtcTimestamp)
                return 1;
            if (this.ArrivalOrder < other.ArrivalOrder)
                return -1;
            return this.ArrivalOrder > other.ArrivalOrder ? 1 : 0;
        }

        public static StructuredEvent EquivalentOf(Guid id) => new StructuredEvent(id, 0U, (IReadOnlyDictionary<string, object>)new Dictionary<string, object>());
    }
}