using System.Text.Json.Serialization;
using System.Diagnostics.CodeAnalysis;

namespace API_Conta_SaldoExtrato.DTO
{
    [ExcludeFromCodeCoverage]    
    public class ListErroDTO
    {
        [JsonPropertyName("erros")]
        public IEnumerable<ErroDTO> Erros { get; set; } = new List<ErroDTO>();
    }

    [ExcludeFromCodeCoverage]
    public class ErroDTO
    {
        internal bool? IcNaoReenviar;

        [JsonPropertyName("codigo")]
        public string? Codigo { get; set; }
        [JsonPropertyName("mensagem")]
        public string? Mensagem { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("exception")]
        public string? Exception { get; set; }
        public string? Orientacao { get; set; }
        public int StatusCode { get; internal set; }
    }
}
