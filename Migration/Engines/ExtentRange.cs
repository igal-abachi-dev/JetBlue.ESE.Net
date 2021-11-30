using JetBlue.ESE.Net.Cache;
using JetBlue.ESE.Net.Time;

namespace JetBlue.ESE.Net.Migration.Engines
{
    public struct ExtentRange : IComparable<ExtentRange>, IEquatable<ExtentRange>
    {
        private readonly DateTime _startUtc;
        private const int ExtentDays = 7;
        private const DayOfWeek FirstDayOfExtent = DayOfWeek.Wednesday;
        public static ExtentRange Empty;

        private ExtentRange(DateTime startUtc)
        {
            if (startUtc.Kind != DateTimeKind.Utc)
                throw new ArgumentException("The startUtc value must be UTC");
            if (startUtc != startUtc.Date)
                throw new ArgumentException("The startUtc value must be at date resolution");
            this._startUtc = startUtc.DayOfWeek == DayOfWeek.Wednesday ? startUtc : throw new ArgumentException("The extent must start on a " + DayOfWeek.Wednesday.ToString());
        }

        public DateTime StartUtc => this._startUtc;

        public DateTime EndUtc => this.StartUtc.AddDays(7.0);

        internal static ExtentRange InclusiveOf(SegmentRange segmentRange) => ExtentRange.InclusiveOf(segmentRange.StartUtc);

        public static ExtentRange InclusiveOfEnd(DateTime utcTimestamp) => utcTimestamp == utcTimestamp.Date && utcTimestamp.DayOfWeek == DayOfWeek.Wednesday ? ExtentRange.EndingAt(utcTimestamp) : ExtentRange.InclusiveOf(utcTimestamp);

        public static ExtentRange EndingAt(DateTime endUtc) => new ExtentRange(endUtc.AddDays(-7.0));

        public static ExtentRange InclusiveOf(DateTime utcTimestamp)
        {
            DateTime date = utcTimestamp.Date;
            int num = (int)(3 - date.DayOfWeek - 7) % 7;
            return new ExtentRange(date.AddDays((double)num));
        }

        public ExtentRange Next() => new ExtentRange(this.StartUtc.AddDays(7.0));

        public ExtentRange Previous() => new ExtentRange(this.StartUtc.AddDays(-7.0));

        public int CompareTo(ExtentRange other) => this.StartUtc.CompareTo(other._startUtc);

        public bool Equals(ExtentRange other) => this._startUtc == other._startUtc;

        public override bool Equals(object obj) => obj is ExtentRange other && this.Equals(other);

        public override int GetHashCode() => this.StartUtc.GetHashCode();

        public static bool operator ==(ExtentRange lhs, ExtentRange rhs) => lhs._startUtc == rhs._startUtc;

        public static bool operator !=(ExtentRange lhs, ExtentRange rhs) => lhs._startUtc != rhs._startUtc;

        public static bool operator >(ExtentRange lhs, ExtentRange rhs) => lhs._startUtc > rhs._startUtc;

        public static bool operator <(ExtentRange lhs, ExtentRange rhs) => lhs._startUtc < rhs._startUtc;

        public static bool operator >=(ExtentRange lhs, ExtentRange rhs) => lhs._startUtc >= rhs._startUtc;

        public static bool operator <=(ExtentRange lhs, ExtentRange rhs) => lhs._startUtc <= rhs._startUtc;

        public override string ToString() => !(this == ExtentRange.Empty) ? this.StartUtc.ToString("o") : "Empty";

        public static ExtentRange Create(DateTime startUtc, DateTime endUtc) => !(startUtc.AddDays(7.0) != endUtc) ? new ExtentRange(startUtc) : throw new ArgumentException("Only " + 7.ToString() + "-day extent ranges are supported");

        public DateTimeRange ToDateTimeRange() => new DateTimeRange(this.StartUtc, this.EndUtc);
    }
}
