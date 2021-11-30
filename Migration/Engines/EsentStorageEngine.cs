

using Microsoft.Isam.Esent.Interop;

namespace JetBlue.ESE.Net.Migration.Engines.Esent
{
    public class EsentStorageEngine : IStorageEngine
    {
        public string ExtentDataFileName => "extent.ESE";

        public static void InitEsent()
        {
            int num1 = (int)Api.JetSetSystemParameter(new JET_INSTANCE(), new JET_SESID(), JET_param.MaxInstances, 1000, (string)null);
            int num2 = (int)Api.JetSetSystemParameter(new JET_INSTANCE(), new JET_SESID(), JET_param.CacheSizeMax, 512, (string)null);
            int num3 = (int)Api.JetSetSystemParameter(new JET_INSTANCE(), new JET_SESID(), JET_param.MaxTemporaryTables, 1024, (string)null);
        }

        public StorageExtent CreateExtent(ExtentRange range, string dataFile) => (StorageExtent)new EsentStorageExtent(range, dataFile, string.Format("Ese-Migrate-{0:n}", (object)Guid.NewGuid()));
    }
}