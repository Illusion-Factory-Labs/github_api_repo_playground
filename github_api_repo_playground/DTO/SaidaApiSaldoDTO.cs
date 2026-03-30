using System.Diagnostics.CodeAnalysis;

namespace API_Conta_SaldoExtrato.DTO
{
    [ExcludeFromCodeCoverage]
    public class SaidaApiSaldoDTO
    {
        public decimal saldo { get; set; }
        public decimal limite { get; set; }
        public decimal saldoBloqueado { get; set; }
        public decimal saldoTotal { get; set; }
    }
}
