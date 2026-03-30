using API_Conta_SaldoExtrato.Config;
using API_Conta_SaldoExtrato.DTO;
using API_Conta_SaldoExtrato.Enums;
using API_Conta_SaldoExtrato.Exceptions;
using API_Conta_SaldoExtrato.Utils;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace API_Conta_SaldoExtrato.Services
{
    /// <summary>
    /// Serviço responsável por operações relacionadas ao saldo.
    /// </summary>
    public class ExtratoService : IExtratoService
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
        public ExtratoService(
            IOptions<ServicesConfig> servicesConfig,
            IOptions<TimeoutConfig> timeoutConfig,
            ApiService apiService)
        {
            _servicesConfig = servicesConfig.Value;
            _apiService = apiService;
            _timeoutConfig = timeoutConfig.Value;
        }       

        public async Task<SaidaApiExtratoDTO> ConsultaExtrato(ExtratoDTO entrada)
        {
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

            string data_inicio;
            string data_fim;
            string url = "";
            bool dataInicioValida = !string.IsNullOrWhiteSpace(entrada.data_inicio) && DateUtil.IsValidDateFormat(entrada.data_inicio);
            bool dataFimValida = !string.IsNullOrWhiteSpace(entrada.data_fim) && DateUtil.IsValidDateFormat(entrada.data_fim);
            if (dataInicioValida && dataFimValida)
            {
                // Ambas as datas foram informadas e são válidas
                data_inicio = entrada.data_inicio;
                data_fim = entrada.data_fim;
            }
            else
            {
                data_inicio = DateTime.Now.AddDays(-45).ToString("yyyy-MM-dd");
                data_fim = DateTime.Now.ToString("yyyy-MM-dd");
            }
            // Monta a URL da API substituindo os placeholders pelos dados da conta            
            url = _servicesConfig.ApiExtrato.Replace("{id_conta}",
                entrada.contaOrigem.unidade.ToString("0") + "-" +
                entrada.contaOrigem.operacao.ToString("0") + "-" +
                entrada.contaOrigem.conta.ToString("0") + "-" +
                entrada.contaOrigem.dv.ToString());
            url += "?";
            url += $"data_inicio={data_inicio}&data_fim={data_fim}";
            url += "&campos=dados_cadastrais,lancamentos,compras,agendamentos,lancamentos_dia";

            // Mensagem de erro padrão para falha na consulta
            string mensagemErro = "Erro ao consultar Extrato, tente novamente";

            // Realiza a chamada GET para a API de saldo
            var httpResponseMessage = await _apiService.SendGetAsync(
                url,
                NomeHttpClient.ApisComAuditoria,
                "SID01",
                mensagemErro,
                _timeoutConfig.TimeoutExtratoMilissegundos);

            // Lê o conteúdo da resposta como string
            var responseContent = await httpResponseMessage.Content.ReadAsStringAsync();

            // Se a resposta for bem-sucedida, desserializa e retorna os dados do saldo
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return JsonSerializer.Deserialize<SaidaApiExtratoDTO>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new SaidaApiExtratoDTO();
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
