using API_Conta_SaldoExtrato.DTO;

namespace API_Conta_SaldoExtrato.Services
{
    /// <summary>
    /// Interface para o serviço de saldo
    /// </summary>
    public interface ISaldoService
    {
        /// <summary>
        /// Consulta o saldo de uma conta
        /// </summary>
        /// <param name="entrada">Dados da consulta</param>
        /// <returns>Resultado padronizado da consulta de saldo</returns>
        Task<RetornoConsultaSaldoDTO> ConsultaSaldo(EntradaPadraoSuperAppDTO entrada);
    }
}
