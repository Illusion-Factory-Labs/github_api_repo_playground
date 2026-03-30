using LIB_Auditoria;
using API_Conta_SaldoExtrato.Config;
using API_Conta_SaldoExtrato.DTO;
using API_Conta_SaldoExtrato.Enums;
using API_Conta_SaldoExtrato.Exceptions;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace API_Conta_SaldoExtrato.Services
{
    /// <summary>
    /// Serviço responsável por realizar chamadas HTTP (GET e POST) para APIs externas,
    /// além de tratar auditoria e exceções relacionadas a essas chamadas.
    /// </summary>
    public class ApiService
    {
        // Fábrica para criação de clientes HTTP
        private readonly IHttpClientFactory _httpClientFactory;
        // Logger para registrar informações e erros
        private readonly ILogger<ApiService> _logger;
        // Nome do header de auditoria
        private readonly string xAuditId = AuditoriaConstantes.HeaderAuditKey;
        // Configuração de timeout das requisições
        private readonly TimeoutConfig _timeoutConfig;
        // Serviço de auditoria para registrar eventos
        private readonly IAuditoriaService _auditoriaService;

        /// <summary>
        /// Construtor que injeta as dependências necessárias.
        /// </summary>
        public ApiService(IHttpClientFactory httpClientFactory, ILogger<ApiService> logger, IOptions<TimeoutConfig> timeoutConfig, IAuditoriaService auditoriaService)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _timeoutConfig = timeoutConfig.Value;
            _auditoriaService = auditoriaService;
        }

        /// <summary>
        /// Realiza uma requisição HTTP GET para a URL informada.
        /// </summary>
        /// <param name="url">URL da API a ser acessada</param>
        /// <param name="nomeHttpClient">Tipo de cliente HTTP</param>
        /// <param name="sistema">Identificação do sistema</param>
        /// <param name="msgErro">Mensagem de erro padrão</param>
        /// <param name="timeoutSegundos">Timeout customizado (opcional)</param>
        /// <returns>Retorna a resposta HTTP da API</returns>
        public virtual async Task<HttpResponseMessage> SendGetAsync(string url, NomeHttpClient nomeHttpClient, string sistema, string msgErro, double? timeoutSegundos = null)
        {
            HttpRequestMessage? request = null;
            var timeout = timeoutSegundos ?? _timeoutConfig.TimeoutPadraoMilissegundos;
            try
            {
                // Cria o cliente HTTP e configura o timeout
                var httpClient = _httpClientFactory.CreateClient(nomeHttpClient.ToString());
                httpClient.Timeout = TimeSpan.FromMilliseconds(timeout);

                // Log de acesso à API
                _logger.LogDebug("Acessando api: {Url}", url.Replace(Environment.NewLine, ""));

                // Monta a requisição GET
                request = new HttpRequestMessage(HttpMethod.Get, url);
                var httpResponseMessage = await httpClient.SendAsync(request);

                // Tenta obter o valor do header de auditoria
                request.Headers.TryGetValues(xAuditId, out var values);

                // Log da resposta da API
                _logger.LogDebug("Saída api {Url}: {XAuditId}: {ValorXAuditId}: {Response}", url.Replace(Environment.NewLine, ""), xAuditId, values?.FirstOrDefault(), await httpResponseMessage.Content.ReadAsStringAsync());

                return httpResponseMessage;
            }
            // Trata timeout da requisição
            catch (TaskCanceledException ex)
            {
                return await ThrowTaskCanceledException(request, sistema, url, ex, timeout, msgErro);
            }
            // Trata erro de requisição HTTP
            catch (HttpRequestException ex)
            {
                return ThrowHttpRequestException(request, sistema, url, ex);
            }
        }

        /// <summary>
        /// Realiza uma requisição HTTP POST para a URL informada, enviando o objeto de entrada.
        /// </summary>
        /// <param name="url">URL da API a ser acessada</param>
        /// <param name="entrada">Objeto a ser enviado no corpo da requisição</param>
        /// <param name="nomeHttpClient">Tipo de cliente HTTP</param>
        /// <param name="sistema">Identificação do sistema</param>
        /// <param name="msgErro">Mensagem de erro padrão</param>
        /// <param name="timeoutSegundos">Timeout customizado (opcional)</param>
        /// <returns>Retorna a resposta HTTP da API</returns>
        public virtual async Task<HttpResponseMessage> SendPostAsync(string url, object entrada, NomeHttpClient nomeHttpClient, string sistema, string msgErro, double? timeoutSegundos = null)
        {
            HttpRequestMessage? request = null;
            var timeout = timeoutSegundos ?? _timeoutConfig.TimeoutPadraoMilissegundos;
            try
            {
                // Cria o cliente HTTP e configura o timeout
                var httpClient = _httpClientFactory.CreateClient(nomeHttpClient.ToString());
                httpClient.Timeout = TimeSpan.FromMilliseconds(timeout);

                // Log de acesso à API com os dados enviados
                _logger.LogInformation("Acessando api {Url} - dados: {Dados}", url.Replace(Environment.NewLine, ""), JsonSerializer.Serialize(entrada));

                // Monta a requisição POST
                request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Content = JsonContent.Create(entrada);
                var httpResponseMessage = await httpClient.SendAsync(request);

                // Tenta obter o valor do header de auditoria
                request.Headers.TryGetValues(xAuditId, out var values);

                // Log da resposta da API
                _logger.LogInformation("Saída api {Url}: {XAuditId}: {ValorXAuditId}: {Response}", url.Replace(Environment.NewLine, ""), xAuditId, values?.FirstOrDefault(), await httpResponseMessage.Content.ReadAsStringAsync());

                return httpResponseMessage;
            }
            // Trata timeout da requisição
            catch (TaskCanceledException ex)
            {
                return await ThrowTaskCanceledException(request, sistema, url, ex, timeout, msgErro);
            }
            // Trata erro de requisição HTTP
            catch (HttpRequestException ex)
            {
                return ThrowHttpRequestException(request, sistema, url, ex);
            }
        }

        /// <summary>
        /// Método privado para tratar exceções de timeout (TaskCanceledException).
        /// Registra auditoria e lança exceção customizada.
        /// </summary>
        private async Task<HttpResponseMessage> ThrowTaskCanceledException(HttpRequestMessage? request, string sistema, string url, TaskCanceledException ex, double timeout, string msgErro)
        {
            var erro = new ErroDTO
            {
                Codigo = $"{sistema}-TMOUT-{timeout}",
                Mensagem = msgErro
            };
            var erroStringJson = JsonSerializer.Serialize(erro);

            IEnumerable<string>? values = null;
            request?.Headers.TryGetValues(xAuditId, out values);

            // Log do erro de timeout
            _logger.LogError(ex, "Saída api {Url}: {XAuditId}: {ValorXAuditId}: {Response}", url.Replace(Environment.NewLine, ""), xAuditId, values?.FirstOrDefault(), erroStringJson);

            // Monta headers para auditoria
            var headers = new Dictionary<string, string[]>();
            var headerAuditKey = values?.FirstOrDefault() ?? Guid.NewGuid().ToString();
            headers.Add(xAuditId, [headerAuditKey]);

            var uri = new Uri(url);
            var acao = "EXTRATO_TIMEOUT";
            var reqBody = request?.Content == null ? "" : await request.Content.ReadAsStringAsync();

            // Envia evento de auditoria (não aguarda retorno)
            _ = _auditoriaService.SendAsync(new AuditoriaDto(uri.PathAndQuery, request?.Method.ToString() ?? "", uri.Host, headers, reqBody, "408", erroStringJson, headers, acao, ""));

            // Lança exceção customizada
            throw new HttpCustomException(erro);
        }

        /// <summary>
        /// Método privado para tratar exceções de requisição HTTP (HttpRequestException).
        /// Registra log e lança exceção customizada.
        /// </summary>
        private HttpResponseMessage ThrowHttpRequestException(HttpRequestMessage? request, string sistema, string url, HttpRequestException ex)
        {
            var erro = new ErroDTO
            {
                Codigo = $"{sistema}-500",
                Mensagem = "Erro ao acessar api."
            };

            IEnumerable<string>? values = null;
            request?.Headers.TryGetValues(xAuditId, out values);

            // Log do erro de requisição
            _logger.LogError(ex, "Saída api {Url}: {XAuditId}: {ValorXAuditId}: {Response}", url.Replace(Environment.NewLine, ""), xAuditId, values?.FirstOrDefault(), JsonSerializer.Serialize(erro));

            // Lança exceção customizada
            throw new HttpCustomException(erro);
        }
    }
}
