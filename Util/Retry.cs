
using Serilog;
using System;


#nullable enable
namespace JetBlue.ESE.Net.Util
{
    public static class Retry
    {
        public static void WithRecovery(int tries, Action @try, Action recover) => Retry.WithRecovery<int>(tries, (Func<int>)(() =>
       {
           @try();
           return 0;
       }), recover);

        public static T WithRecovery<T>(int tries, Func<T> @try, Action recover)
        {
            if (@try == null)
                throw new ArgumentNullException(nameof(@try));
            if (recover == null)
                throw new ArgumentNullException(nameof(recover));
            while (tries > 0)
            {
                --tries;
                try
                {
                    return @try();
                }
                catch (Exception ex) when (tries > 0)
                {
                    Log.Warning<int>(ex, "Operation failed; attempting recovery and re-trying {TriesRemaining} more times", tries);
                    recover();
                }
            }
            throw new ArgumentOutOfRangeException(nameof(tries), "The operation must be attempted at least once.");
        }
    }
}
