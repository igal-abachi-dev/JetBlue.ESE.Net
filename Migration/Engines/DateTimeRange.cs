namespace JetBlue.ESE.Net.Time
{
    public readonly struct DateTimeRange
    {
        public static readonly DateTimeRange Empty;

        public DateTimeRange(DateTime start, DateTime end)
        {
            if (start.Kind != DateTimeKind.Utc)
                throw new ArgumentException("The range `start` must be UTC time.");
            if (end.Kind != DateTimeKind.Utc)
                throw new ArgumentException("The range `end` must be UTC time.");
            this.Start = !(end < start) ? start : throw new ArgumentException("The range `start` must be before or equal to the range `end`.");
            this.End = end;
        }

        public DateTime Start { get; }

        public DateTime End { get; }

        public TimeSpan Duration => this.End - this.Start;

        public DateTimeRangeBounds ToBounds() => new DateTimeRangeBounds(new DateTime?(this.Start), new DateTime?(this.End));
    }
}
