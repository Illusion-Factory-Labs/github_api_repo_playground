using System.Diagnostics.CodeAnalysis;

namespace API_Conta_SaldoExtrato.Config
{
    [ExcludeFromCodeCoverage]
    public class ServicesConfig
    {
        public string? ApiSaldo { get; set; }
        public string? ApiExtrato { get; set; }
    }
}
