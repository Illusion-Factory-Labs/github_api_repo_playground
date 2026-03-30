using API_Conta_SaldoExtrato.DTO;

namespace API_Conta_SaldoExtrato.Services
{
    /// <summary>
    /// Interface para o serviço de API externa
    /// </summary>
    public interface IApiService
    {
        /// <summary>
        /// Consulta saldo através da API externa
        /// </summary>
        /// <param name="entrada">Dados da consulta</param>
        /// <returns>Resultado da consulta</returns>
        Task<SaidaApiSaldoDTO> ConsultarSaldoAsync(EntradaPadraoSuperAppDTO entrada);
    }
}
