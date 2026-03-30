using API_Conta_SaldoExtrato.Config;
using API_Conta_SaldoExtrato.DTO;
using API_Conta_SaldoExtrato.Security;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Middleware responsável por validar regras de segurança e predecessão de etapas via Redis.
/// Realiza validação de etapas anteriores, insere etapa atual no cache e retorna erro de acesso quando necessário.
/// </summary>
public class SegurancaMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SegurancaMiddleware> _logger;
    private readonly HttpClient _httpClient;
    private readonly RedisConnectionHelper _redisConnectionHelper;
    private readonly CacheConfig _cacheConfig;

    // URLs de serviços externos de segurança (validação e criptografia)
    private readonly string _urlValidacaoSeguranca = "https://servico-externo/validar";
    private readonly string _urlCriptografia = "https://servico-externo/criptografar";
    // Prefixo para chave de cache no Redis
    private readonly string _chaveCache = "PREDECESSAO_";

    private readonly string _ssoLoginx = "loginx";

    /// <summary>
    /// Construtor do middleware, recebe dependências via injeção.
    /// </summary>
    public SegurancaMiddleware(RequestDelegate proximo, ILogger<SegurancaMiddleware> logger, HttpClient httpClient, RedisConnectionHelper redisConnectionHelper, IOptions<CacheConfig> cacheConfig)
    {
        _next = proximo;
        _logger = logger;
        _httpClient = httpClient;
        _redisConnectionHelper = redisConnectionHelper;
        _cacheConfig = cacheConfig.Value;
    }

    /// <summary>
    /// Método principal do middleware, executado a cada requisição.
    /// Valida regras de predecessão antes e insere etapa atual após o processamento.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        // TODO: Chamar API de segurança para decriptografar, validar e recuperar token
        EntradaPadraoSuperAppDTO? requestPadrao = null;
        if (context.Request.ContentLength > 0 && context.Request.ContentType?.Contains("application/json") == true)
        {
            context.Request.EnableBuffering();
            string bodyContent;
        
            using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true))
            {
                bodyContent = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0; // Reset para próximo middleware
            }

            if (!string.IsNullOrWhiteSpace(bodyContent))
            {
                try
                {
                    requestPadrao = JsonSerializer.Deserialize<EntradaPadraoSuperAppDTO>(bodyContent, new JsonSerializerOptions 
                    { 
                        PropertyNameCaseInsensitive = true,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                    });
                    //_logger.LogInformation("Body deserializado com sucesso. CPF Cliente: {CpfCliente}", requestPadrao?.cpfCliente);
                }
                catch (JsonException ex)
                {
                    //_logger.LogWarning("Erro ao deserializar body da requisição: {Erro}", ex.Message);
                    requestPadrao = new EntradaPadraoSuperAppDTO();
                }
            }
            else
            {
                requestPadrao = new EntradaPadraoSuperAppDTO();
            }
        }
        // Recupera atributo de predecessão do endpoint
        var atributte = context.GetEndpoint()?.Metadata.GetMetadata<PredecessaoAttribute>();
        // Se o método está anotado com [Predecessao], valida regras de predecessão
        if (atributte != null)
        {
            await ValidaPredecessao(context, atributte);
        }
        var atributteAssinatura = context.GetEndpoint()?.Metadata.GetMetadata<AssinaturaEletronicaAttribute>();
        // Se o método está anotado com [Predecessao], valida regras de predecessão
        if (atributteAssinatura != null)
        {
            //TODO, validar se o body contem assinatura eletronica preenchida e chamar API para validação
        }

        
        //abre o token do header authoriation e recupera a claim preferred_username se ela nao for um cpf retorna erro e seta no body essa claim como a propriedade cpfCliente
        var authorizationHeader = context.Request.Headers["Authorization"].ToString();
        if (!string.IsNullOrWhiteSpace(authorizationHeader))
        {
            var token = authorizationHeader.Split(' ')[1];
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            
            //var issuer = jwtToken.Claims.FirstOrDefault(c => c.Type == "iss")?.Value;
            string? cpfInToken;
            if (jwtToken.Issuer.Contains(_ssoLoginx))
            {
                cpfInToken = jwtToken.Claims.FirstOrDefault(c => c.Type == "preferred_username_cpf")?.Value;
            }
            else
            {
                cpfInToken = jwtToken.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;
            }
            
            if (string.IsNullOrWhiteSpace(cpfInToken))
            {
                await ErroAcesso(context);
                return;
            }

            //Lendo cpf presente no body da request
            var cpfInRequest = await ReadCpfFromRequestBody(context);
            
            if (!cpfInToken.Equals(cpfInRequest))
            {
                _logger.LogInformation("CPF do token nao bate cpf do request: {CpfCliente}", cpfInRequest);
                await ErroAcesso(context);
                return;
            } 
            
        }

        // Chama o próximo middleware da pipeline
        await _next(context);

        // Após processamento, insere etapa atual no Redis se houver atributo de predecessão
        if (atributte != null)
        {
            await SetaPredecessao(context, atributte);
        }
        // TODO: Chamar API de segurança para criptografar e assinar
    }

    // Lê o body original, deserializa, e retornar o cpf presente na request
    private static async Task<string?> ReadCpfFromRequestBody(HttpContext context)
    {
        try
        {
            context.Request.EnableBuffering();
            string bodyStr;
            using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true))
            {
                bodyStr = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
            }

            if (string.IsNullOrWhiteSpace(bodyStr))
            {
                return null;
            }

            var entrada = JsonSerializer.Deserialize<EntradaPadraoSuperAppDTO>(bodyStr, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return entrada?.cpfCliente.ToString();
        }
        catch
        {
            return null;
        }
    }
  
    private static async Task SetaCpfToken(HttpContext context, long cpfCliente)
    {
        // Lê o body original, deserializa, atualiza o cpfCliente e reescreve o body
        context.Request.EnableBuffering();
        string bodyStr;
        using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true))
        {
            bodyStr = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
        }

        EntradaPadraoSuperAppDTO? entrada;
        if (!string.IsNullOrWhiteSpace(bodyStr))
        {
            entrada = JsonSerializer.Deserialize<EntradaPadraoSuperAppDTO>(bodyStr, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        else
        {
            entrada = new EntradaPadraoSuperAppDTO();
        }

        entrada.cpfCliente = cpfCliente;

        // Serializa novamente e substitui o body da requisição
        var novoBody = JsonSerializer.Serialize(entrada);
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(novoBody));
        context.Request.Body.Position = 0;
    }

    /// <summary>
    /// Insere a etapa atual no Redis, associando ao SessionId.
    /// </summary>
    private async Task SetaPredecessao(HttpContext context, PredecessaoAttribute atributte)
    {
        // Recupera SessionId do header
        var sessionId = context.Request.Headers["SessionId"].ToString();
        // Adiciona etapa atual no Redis com tempo de expiração configurado
        await _redisConnectionHelper.setRegister(_chaveCache + sessionId, atributte.AposChamada.ToString(), _cacheConfig.TTLPredecessao);
        _logger.LogInformation("SessionId {SessionId} adicionado ao Redis.", sessionId);
    }

    /// <summary>
    /// Valida se o SessionId possui as etapas predecessoras necessárias no Redis.
    /// Retorna erro de acesso caso não esteja conforme as regras.
    /// </summary>
    private async Task ValidaPredecessao(HttpContext context, PredecessaoAttribute atributte)
    {
        // Recupera SessionId do header
        var sessionId = context.Request.Headers["SessionId"].ToString();

        // Se SessionId não informado, retorna erro de acesso
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            await ErroAcesso(context);
        }
        // Se há regras de predecessão, valida etapa atual no Redis
        if (atributte.Anteriores.Count() > 0)
        {
            // Recupera etapa atual do cache
            var statusAtual = await _redisConnectionHelper.getRegister(_chaveCache + sessionId);
            if (statusAtual == null || statusAtual == "")
            {
                _logger.LogWarning("SessionId {SessionId} não encontrado no Redis.", sessionId);
                await ErroAcesso(context);
            }
            // Converte etapa atual para inteiro
            var etapaAtual = int.Parse(statusAtual);
            // Verifica se etapa atual está na lista de predecessoras permitidas
            if (!atributte.Anteriores.Contains(etapaAtual))
            {
                _logger.LogWarning("SessionId {SessionId} não é permitido para esta etapa.", sessionId);
                await ErroAcesso(context);
            }
        }
    }

    /// <summary>
    /// Retorna erro de acesso 403 em formato JSON.
    /// </summary>
    private async Task ErroAcesso(HttpContext context)
    {
        var jsonSerializerOptions = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
        _logger.LogWarning("SessionId não informado no header da requisição.");
        var erro = new List<ErroDTO>
                            {
                                new ErroDTO
                                {
                                    StatusCode= 403,
                                    Codigo ="E_SEG_PRE_403",
                                    Mensagem = "Erro de Acesso",
                                    IcNaoReenviar=false
                                }

                };
        context.Response.StatusCode = erro.First().StatusCode;
        context.Response.ContentType = "application/json";
        // Retorna os erros no formato JSON
        await context.Response.WriteAsync(
            Encoding.UTF8.GetString(
                JsonSerializer.SerializeToUtf8Bytes(
                    erro, jsonSerializerOptions)));
    }
}
