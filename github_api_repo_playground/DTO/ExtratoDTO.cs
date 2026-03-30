using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace API_Conta_SaldoExtrato.DTO
{
    /// <summary>
    /// DTO específico para consulta de extrato, herdando da estrutura padrão do SuperApp
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ExtratoDTO : EntradaPadraoSuperAppDTO
    {
        /// <summary>
        /// Data de início para consulta do extrato no formato yyyy-MM-dd
        /// </summary>
        [JsonPropertyName("data_inicio")]
        public string? data_inicio { get; set; }

        /// <summary>
        /// Data de fim para consulta do extrato no formato yyyy-MM-dd
        /// </summary>
        [JsonPropertyName("data_fim")]
        public string? data_fim { get; set; }
    }
}