using Microsoft.AspNetCore.Mvc.Filters;
using StackExchange.Redis;

/// <summary>
/// Indica que o método da controller possui regras de predecessão para execução.
/// Permite definir quais etapas anteriores (Anteriores) são necessárias antes de acessar a etapa atual (AposChamada).
/// Usado para controlar o fluxo de chamadas e validação de etapas via middleware.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class PredecessaoAttribute : ActionFilterAttribute
{
    /// <summary>
    /// Lista de etapas anteriores que devem ser concluídas antes de acessar o método anotado.
    /// </summary>
    public int[] Anteriores { get; set; } = [];

    /// <summary>
    /// Identificador da etapa atual após a chamada do método.
    /// </summary>
    public int AposChamada { get; set; }

    /// <summary>
    /// Construtor que recebe etapas anteriores e a etapa atual.
    /// </summary>
    /// <param name="Anteriores">Etapas predecessoras necessárias.</param>
    /// <param name="AposChamada">Etapa atual após chamada.</param>
    public PredecessaoAttribute(int[] Anteriores, int AposChamada)
    {
        this.Anteriores = Anteriores;
        this.AposChamada = AposChamada;
    }

    /// <summary>
    /// Construtor que recebe apenas a etapa atual.
    /// </summary>
    /// <param name="AposChamada">Etapa atual após chamada.</param>
    public PredecessaoAttribute(int AposChamada)
    {
        this.AposChamada = AposChamada;
    }
}
