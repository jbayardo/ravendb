using Lextm.SharpSnmpLib;
using Raven.Client.Util;
using Raven.Server.Documents;

namespace Raven.Server.Monitoring.Snmp.Objects.Database
{
    public class DatabaseIndexTimeSinceLastIndexing : DatabaseIndexScalarObjectBase<TimeTicks>
    {
        public DatabaseIndexTimeSinceLastIndexing(string databaseName, string indexName, DatabasesLandlord landlord, int databaseIndex, int indexIndex)
            : base(databaseName, indexName, landlord, databaseIndex, indexIndex, "10")
        {
        }

        protected override TimeTicks GetData(DocumentDatabase database)
        {
            var index = GetIndex(database);
            var stats = index.GetStats();

            if (stats.LastIndexingTime.HasValue)
                return new TimeTicks(SystemTime.UtcNow - stats.LastIndexingTime.Value);

            return null;
        }
    }
}
