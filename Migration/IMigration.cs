
namespace JetBlue.ESE.Net.Migration
{
    public interface IMigration
    {
        void Apply(DocumentSession session);
    }
}
