
namespace JetBlue.ESE.Net.Documents
{
    public static class JetBlueDocument
    {
        public static string GetIdPrefix<TDocument>() => typeof(TDocument) == typeof(object) ? "" : typeof(TDocument).Name.ToLowerInvariant() + "-";
    }
}
