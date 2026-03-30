using System.Diagnostics.CodeAnalysis;

namespace API_Conta_SaldoExtrato.DTO
{
    [ExcludeFromCodeCoverage]
    public class RetornoConsultaExtratoDTO
    {
        public string? conta { get; set; }        

        public List<LancamentoDTO>? lancamentos { get; set; }
        
        public List<LancamentoDiaDTO>? lancamentos_dia { get; set; }

    }
}
