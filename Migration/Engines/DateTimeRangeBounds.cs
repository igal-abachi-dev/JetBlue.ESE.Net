namespace JetBlue.ESE.Net.Time
{
    public readonly struct DateTimeRangeBounds
    {
        public static readonly DateTimeRangeBounds Unbounded;

        public DateTimeRangeBounds(DateTime? start, DateTime? end)
        {
            if (start.HasValue && start.Value.Kind != DateTimeKind.Utc)
                throw new ArgumentException("The range `start` must be UTC time.");
            if (end.HasValue && end.Value.Kind != DateTimeKind.Utc)
                throw new ArgumentException("The range `end` must be UTC time.");
            if (start.HasValue && end.HasValue)
            {
                DateTime? nullable1 = end;
                DateTime? nullable2 = start;
                if ((nullable1.HasValue & nullable2.HasValue ? (nullable1.GetValueOrDefault() < nullable2.GetValueOrDefault() ? 1 : 0) : 0) != 0)
                    throw new Exception(string.Format("The range `start` ({0:o}) must be before or equal to the range `end` ({1:o}).", (object)start, (object)end));
            }
            this.Start = start;
            this.End = end;
        }

        public DateTime? Start { get; }

        public DateTime? End { get; }

        public DateTimeRangeBounds? Over(DateTimeRange range)
        {
            DateTime? start = new DateTime?();
            DateTime? end = new DateTime?();
            DateTime? nullable;
            if (this.Start.HasValue)
            {
                nullable = this.Start;
                if (nullable.Value >= range.End)
                    return new DateTimeRangeBounds?();
                nullable = this.Start;
                if (nullable.Value > range.Start)
                {
                    ref DateTime? local = ref start;
                    nullable = this.Start;
                    DateTime dateTime = nullable.Value;
                    local = new DateTime?(dateTime);
                }
            }
            nullable = this.End;
            if (nullable.HasValue)
            {
                nullable = this.End;
                if (nullable.Value <= range.Start)
                    return new DateTimeRangeBounds?();
                nullable = this.End;
                if (nullable.Value < range.End)
                {
                    ref DateTime? local = ref end;
                    nullable = this.End;
                    DateTime dateTime = nullable.Value;
                    local = new DateTime?(dateTime);
                }
            }
            return new DateTimeRangeBounds?(new DateTimeRangeBounds(start, end));
        }

        public DateTimeRangeBounds WithStart(DateTime? start) => new DateTimeRangeBounds(start, this.End);

        public DateTimeRangeBounds WithEnd(DateTime? end) => new DateTimeRangeBounds(this.Start, end);

        public bool Includes(DateTime dateTime)
        {
            if (dateTime.Kind != DateTimeKind.Utc)
                throw new ArgumentException("The `dateTime` must be UTC time.");
            DateTime? nullable = this.Start;
            if (nullable.HasValue)
            {
                DateTime dateTime1 = dateTime;
                nullable = this.Start;
                DateTime dateTime2 = nullable.Value;
                if (!(dateTime1 >= dateTime2))
                    return false;
            }
            nullable = this.End;
            if (!nullable.HasValue)
                return true;
            DateTime dateTime3 = dateTime;
            nullable = this.End;
            DateTime dateTime4 = nullable.Value;
            return dateTime3 < dateTime4;
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("[");
            if (this.Start.HasValue)
                stringBuilder.Append((object)this.Start.Value);
            stringBuilder.Append("..");
            if (this.End.HasValue)
                stringBuilder.Append((object)this.End.Value);
            stringBuilder.Append(")");
            return stringBuilder.ToString();
        }
    }
}
