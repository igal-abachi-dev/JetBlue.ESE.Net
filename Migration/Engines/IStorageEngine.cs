

namespace JetBlue.ESE.Net.Migration.Engines
{
    public interface IStorageEngine
    {
        string ExtentDataFileName { get; }

        StorageExtent CreateExtent(ExtentRange range, string dataFile);
    }
}