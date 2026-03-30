using System.Diagnostics.CodeAnalysis;

namespace API_Conta_SaldoExtrato.Config
{
    [ExcludeFromCodeCoverage]
    public class TimeoutConfig
    {
        public double TimeoutSaldoMilissegundos { get; set; }
        public double TimeoutExtratoMilissegundos { get; set; }
        public double TimeoutPadraoMilissegundos { get; set; }
    }
}
