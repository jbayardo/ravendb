using Lextm.SharpSnmpLib;
using Raven.Server.Documents;

namespace Raven.Server.Monitoring.Snmp.Objects.Database
{
    public class DatabaseIndexLastIndexingTime : DatabaseIndexScalarObjectBase<OctetString>
    {
        public DatabaseIndexLastIndexingTime(string databaseName, string indexName, DatabasesLandlord landlord, int databaseIndex, int indexIndex)
            : base(databaseName, indexName, landlord, databaseIndex, indexIndex, "8")
        {
        }

        protected override OctetString GetData(DocumentDatabase database)
        {
            var index = GetIndex(database);
            var stats = index.GetStats();
            if (stats.LastIndexingTime.HasValue)
                return new OctetString(stats.LastIndexingTime.ToString());

            return null;
        }
    }
}
