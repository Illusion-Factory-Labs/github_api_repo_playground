using LIB_Auditoria;
using API_Conta_SaldoExtrato.DTO;
using API_Conta_SaldoExtrato.Security;
using API_Conta_SaldoExtrato.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API_Conta_SaldoExtrato.Controllers
{
    // Define que esta classe é um controller de API e configura a rota base
    [ApiController]
    [Route("conta-saldoextrato/v1")]
    public class SaldoController : Controller
    {
        // Logger para registrar informações e erros do controller
        private readonly ILogger<SaldoController> _logger;
        // Serviço responsável pelas operações de saldo
        private readonly ISaldoService _saldoService;

        /// <summary>
        /// Construtor que injeta as dependências necessárias no controller.
        /// </summary>
        public SaldoController(ILogger<SaldoController> logger, ISaldoService saldoService)
        {
            _logger = logger;
            _saldoService = saldoService;
        }

        /// <summary>
        /// Endpoint HTTP POST para consultar o saldo.
        /// Recebe os dados da conta no corpo da requisição.
        /// </summary>
        /// <param name="entrada">Dados de entrada para consulta de saldo</param>
        /// <returns>Retorna os dados de saldo obtidos do serviço</returns>
        // [HttpPost("saldo")]
        // [Authorize]
        // //[Predecessao([1], 2)]
        // [Auditoria(Acao = "SALDO_CONTA")] // Realiza auditoria da ação
        // public async Task<RetornoConsultaSaldoDTO> GetSaldo([FromBodyAttribute] EntradaPadraoSuperAppDTO entrada)
        // {
        //     // Chama o serviço de saldo e retorna o resultado
        //     return await _saldoService.ConsultaSaldo(entrada);
        // }

        [HttpGet("helloWorld")]
        public string GetHelloWorld()
        {
            return "Hello World - com PR Review e Copilot";
        }
    }
}
