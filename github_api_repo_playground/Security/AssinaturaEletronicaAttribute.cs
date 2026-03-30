using System;

namespace API_Conta_SaldoExtrato.Security
{
    /// <summary>
    /// Indica que o m�todo da controller requer valida��o de assinatura eletr�nica.
    /// Pode ser utilizado por middlewares ou filtros para aplicar a valida��o necess�ria.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class AssinaturaEletronicaAttribute : Attribute
    {
       
    }
}
