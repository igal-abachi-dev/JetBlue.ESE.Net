
using System;
using System.Collections.Generic;

namespace JetBlue.ESE.Net.Migration
{
    public class MigrationState
    {
        public Dictionary<string, DateTime> AppliedMigrations { get; } = new Dictionary<string, DateTime>();

        public static string GetMigrationKey(MigrationMetadata migration) => migration.Name;
    }

}
