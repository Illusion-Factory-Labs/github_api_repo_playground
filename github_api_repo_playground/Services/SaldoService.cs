using API_Conta_SaldoExtrato.Config;
using API_Conta_SaldoExtrato.DTO;
using API_Conta_SaldoExtrato.Enums;
using API_Conta_SaldoExtrato.Exceptions;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace API_Conta_SaldoExtrato.Services
{
    /// <summary>
    /// Serviço responsável por operações relacionadas ao saldo.
    /// </summary>
    public class SaldoService: ISaldoService
    {
        // Configuração dos serviços externos
        private readonly ServicesConfig _servicesConfig;
        // Serviço para chamadas HTTP
        private readonly ApiService _apiService;
        // Configuração de timeout das requisições
        private readonly TimeoutConfig _timeoutConfig;
        
        /// <summary>
        /// Construtor que injeta as dependências necessárias.
        /// </summary>
        public SaldoService(
            IOptions<ServicesConfig> servicesConfig,
            IOptions<TimeoutConfig> timeoutConfig,
            ApiService apiService)
        {
            _servicesConfig = servicesConfig.Value;
            _apiService = apiService;
            _timeoutConfig = timeoutConfig.Value;
        }

        /// <summary>
        /// Consulta o saldo de uma conta informada.
        /// </summary>
        /// <param name="entrada">Dados de entrada contendo informações da conta.</param>
        /// <returns>Retorna os dados de saldo obtidos da API.</returns>
        /// <exception cref="BusinessException">Lançada quando os dados de entrada são inválidos ou ocorre erro na consulta.</exception>
        public async Task<RetornoConsultaSaldoDTO> ConsultaSaldo(EntradaPadraoSuperAppDTO entrada)
        {
            // Valida se a conta de origem está preenchida corretamente, senão retorna erro 400
            if (entrada.contaOrigem == null || entrada.contaOrigem.unidade <= 0 || entrada.contaOrigem.operacao <= 0 || entrada.contaOrigem.conta <= 0)
            {
                throw new BusinessException(new ListErroDTO
                {
                    Erros = new List<ErroDTO>
                            {
                                new ErroDTO
                                {
                                    StatusCode= 400,
                                    Codigo ="E_API_SPA_400",
                                    Mensagem = "Dados da conta de origem inválidos.",
                                    IcNaoReenviar=false
                                }
                            }
                });
            }

            // Monta a URL da API substituindo os placeholders pelos dados da conta
            string url = _servicesConfig.ApiSaldo
                .Replace("{agencia}", entrada.contaOrigem.unidade.ToString("0"))
                .Replace("{produto}", entrada.contaOrigem.operacao.ToString("0"))
                .Replace("{numero}", entrada.contaOrigem.conta.ToString("0"))
                .Replace("{dv}", entrada.contaOrigem.dv.ToString());
            url += "?campos=limite;saldo_bloqueado;saldo_total;saldo_disponivel_com_limite;saldo_poupanca_integrada;saldo_proprio_disponivel;saldo_total";
                        
            // Mensagem de erro padrão para falha na consulta
            string mensagemErro = "Erro ao consultar Saldo, tente novamente";

            // Realiza a chamada GET para a API de saldo
            var httpResponseMessage = await _apiService.SendGetAsync(
                url,
                NomeHttpClient.ApisComAuditoria,
                "SID01",
                mensagemErro,
                _timeoutConfig.TimeoutSaldoMilissegundos);

            // Lê o conteúdo da resposta como string
            var responseContent = await httpResponseMessage.Content.ReadAsStringAsync();

            // Se a resposta for bem-sucedida, desserializa e retorna os dados do saldo
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<RetornoConsultaSaldoDTO>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new RetornoConsultaSaldoDTO();
            }
            else
            {
                // Se houver erro na resposta, lança exceção de negócio com os detalhes do erro
                throw new BusinessException(new ListErroDTO
                {
                    Erros = new List<ErroDTO>
                            {
                                new ErroDTO
                                {
                                    StatusCode= (int) httpResponseMessage.StatusCode,
                                    Codigo ="E_API_SID_"+httpResponseMessage.StatusCode.ToString(),
                                    Mensagem = mensagemErro,
                                    IcNaoReenviar=false
                                }
                            }
                });
            }
        }
    }
}
