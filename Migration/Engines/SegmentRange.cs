
namespace JetBlue.ESE.Net.Cache
{
    internal struct SegmentRange : IComparable<SegmentRange>, IEquatable<SegmentRange>
    {
        private readonly DateTime _startUtc;
        private const int HoursDuration = 1;
        private const long DurationTicks = 36000000000;
        public static SegmentRange Empty;

        private SegmentRange(DateTime startUtc)
        {
            if (startUtc.Kind != DateTimeKind.Utc)
                throw new ArgumentException("The `fromUtcInclusive` value must be UTC.");
            this._startUtc = !(startUtc != SegmentRange.StartInclusiveOf(startUtc)) ? startUtc : throw new ArgumentException("The `fromUtcInclusive` value must be at segment resolution.");
        }

        public DateTime StartUtc => this._startUtc;

        public DateTime EndUtc => this._startUtc.AddTicks(36000000000L);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static DateTime StartInclusiveOf(DateTime utcTimestamp)
        {
            TimeSpan timeOfDay = utcTimestamp.TimeOfDay;
            long num = timeOfDay.Ticks - timeOfDay.Ticks % 36000000000L;
            return utcTimestamp.Date.AddTicks(num);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SegmentRange InclusiveOf(DateTime utcTimestamp) => new SegmentRange(SegmentRange.StartInclusiveOf(utcTimestamp));

        public static SegmentRange InclusiveOfEnd(DateTime utcTimestamp)
        {
            SegmentRange segmentRange = SegmentRange.InclusiveOf(utcTimestamp);
            return utcTimestamp == segmentRange.StartUtc ? segmentRange.Previous() : segmentRange;
        }

        public static SegmentRange EndingAt(DateTime endUtc) => new SegmentRange(endUtc.AddTicks(-36000000000L));

        public SegmentRange Next() => new SegmentRange(this._startUtc.AddTicks(36000000000L));

        public SegmentRange Previous() => new SegmentRange(this._startUtc.AddTicks(-36000000000L));

        public int CompareTo(SegmentRange other) => this._startUtc.CompareTo(other._startUtc);

        public bool Equals(SegmentRange other) => this._startUtc == other._startUtc;

        public override bool Equals(object obj) => obj is SegmentRange other && this.Equals(other);

        public override int GetHashCode() => this._startUtc.GetHashCode();

        public static bool operator ==(SegmentRange lhs, SegmentRange rhs) => lhs._startUtc == rhs._startUtc;

        public static bool operator !=(SegmentRange lhs, SegmentRange rhs) => lhs._startUtc != rhs._startUtc;

        public static bool operator >(SegmentRange lhs, SegmentRange rhs) => lhs._startUtc > rhs._startUtc;

        public static bool operator <(SegmentRange lhs, SegmentRange rhs) => lhs._startUtc < rhs._startUtc;

        public static bool operator >=(SegmentRange lhs, SegmentRange rhs) => lhs._startUtc >= rhs._startUtc;

        public static bool operator <=(SegmentRange lhs, SegmentRange rhs) => lhs._startUtc <= rhs._startUtc;

        public override string ToString() => !(this == SegmentRange.Empty) ? this._startUtc.ToString("o", (IFormatProvider)CultureInfo.InvariantCulture) : "Empty";

        public DateTimeRange ToDateTimeRange() => new DateTimeRange(this.StartUtc, this.EndUtc);
    }
}