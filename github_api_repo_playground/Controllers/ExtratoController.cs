//using LIB_Auditoria;
using API_Conta_SaldoExtrato.DTO;
using API_Conta_SaldoExtrato.Security;
using API_Conta_SaldoExtrato.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Swashbuckle.AspNetCore.Annotations;
using LIB_Auditoria;



namespace API_Conta_SaldoExtrato.Controllers
{
    // Define que esta classe é um controller de API e configura a rota base
    [ApiController]
    [Route("conta-saldoextrato/v1")]
    [Produces("application/json")]
    public class ExtratoController : Controller
    {
        // Logger para registrar informações e erros do controller
        private readonly ILogger<ExtratoController> _logger;
        // Serviço responsável pelas operações de saldo
        private readonly IExtratoService _extratoService;

        /// <summary>
        /// Construtor que injeta as dependências necessárias no controller.
        /// </summary>
        public ExtratoController(ILogger<ExtratoController> logger, IExtratoService extratoService)
        {
            _logger = logger;
            _extratoService = extratoService;
        }

        /// <summary>
        /// Endpoint HTTP POST para consultar o saldo.
        /// Recebe os dados da conta no corpo da requisição.
        /// </summary>
        /// <param name="entrada">Dados de entrada para consulta de saldo</param>
        /// <returns>Retorna os dados de saldo obtidos do serviço</returns>
        // [Authorize]
        // [HttpPost("extrato")]
        // [Auditoria(Acao = "EXTRATO_CONTA")] // Realiza auditoria da ação
        // [SwaggerResponse(StatusCodes.Status200OK, "Extrato consultada com sucesso", typeof(RetornoConsultaExtratoDTO))]
	    // [SwaggerResponse(StatusCodes.Status401Unauthorized, "Não autenticado")]
	    // [SwaggerResponse(StatusCodes.Status403Forbidden, "Não autorizado")]
	    // [SwaggerResponse(StatusCodes.Status500InternalServerError, "Erro ao consultar extrato")]
        // //[Predecessao([1], 2)]
        // //[Auditoria(Acao = "SALDO_CONTA")] // Realiza auditoria da ação
        // public async Task<RetornoConsultaExtratoDTO> GetExtrato([FromBody] ExtratoDTO entrada)
        // {           
        //     // Verificação adicional para debug
        //     if (string.IsNullOrEmpty(entrada.data_inicio) && string.IsNullOrEmpty(entrada.data_fim))
        //     {
        //         _logger.LogWarning("ATENÇÃO: As datas data_inicio e data_fim estão nulas ou vazias!");
        //     }
        //     SaidaApiExtratoDTO saidaExtrato = await _extratoService.ConsultaExtrato(entrada);
        //     RetornoConsultaExtratoDTO retornoExtrato = new RetornoConsultaExtratoDTO
        //     {
        //         conta = saidaExtrato.conta,
        //         lancamentos = saidaExtrato.lancamentos,
        //         lancamentos_dia = saidaExtrato.lancamentos_dia               
        //     };
        //     return retornoExtrato;
        // }

    }
}
