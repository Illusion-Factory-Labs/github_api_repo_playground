using API_Conta_SaldoExtrato.DTO;

namespace API_Conta_SaldoExtrato.Services
{
    /// <summary>
    /// Interface para o serviço de extrato
    /// </summary>
    public interface IExtratoService
    {
        /// <summary>
        /// Consulta o extrato de uma conta
        /// </summary>
        /// <param name="entrada">Dados da consulta</param>
        /// <returns>Resultado padronizado da consulta de extrato</returns>
       
       Task<SaidaApiExtratoDTO> ConsultaExtrato(ExtratoDTO entrada);
    }
}
