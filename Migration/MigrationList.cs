
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


#nullable enable
namespace JetBlue.ESE.Net.Migration
{
    public class MigrationList : IEnumerable<Lazy<IMigration, MigrationMetadata>>, IEnumerable
    {
        private readonly IEnumerable<Lazy<IMigration, MigrationMetadata>> _migrations;

        public static MigrationList Empty => new MigrationList(Enumerable.Empty<Lazy<IMigration, MigrationMetadata>>());

        public MigrationList(
          IEnumerable<Lazy<IMigration, MigrationMetadata>> migrations)
        {
            this._migrations = migrations;
        }

        public IEnumerator<Lazy<IMigration, MigrationMetadata>> GetEnumerator() => this._migrations.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this._migrations.GetEnumerator();
    }
}
