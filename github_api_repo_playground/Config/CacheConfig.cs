using System.Diagnostics.CodeAnalysis;

namespace API_Conta_SaldoExtrato.Config
{
    [ExcludeFromCodeCoverage]
    public class CacheConfig
    {
        public string Connection { get; set; } = string.Empty;
        public int TTLPredecessao { get; set; }
        public int TTLPadrao { get; set; }
    }
}
