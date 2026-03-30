using Swashbuckle.AspNetCore.Annotations;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace API_Conta_SaldoExtrato.Dto.Error;

[ExcludeFromCodeCoverage]
public class Erro
{
    [SwaggerSchema(Description = "Código de erro.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Codigo { get; set; }

    [SwaggerSchema(Description = "Mensagem de erro.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Mensagem { get; set; }

    [SwaggerSchema(Description = "Orientação sobre o erro.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Orientacao { get; set; }

    [SwaggerSchema(Description = "Tipo de crítica da elegibilidade do cliente.")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? IcCriticaElegCliente { get; set; }

    [SwaggerIgnore]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public bool? icNaoReenviar { get; set; }
}
