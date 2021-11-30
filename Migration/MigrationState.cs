
using System;
using System.Collections.Generic;

namespace JetBlue.ESE.Net.Migration
{
    public class MigrationState
    {
        public Dictionary<string, DateTime> AppliedMigrations { get; } = new Dictionary<string, DateTime>();

        public static string GetMigrationKey(MigrationMetadata migration)
        {
            string str = migration.Name;
            if (migration.IsVersionSpecific)
                str = str + " - " + migration.Type.Assembly.GetName().Version?.ToString();
            return str;
        }
    }
}
