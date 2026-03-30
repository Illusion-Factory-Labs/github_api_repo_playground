using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using LIB_Auditoria;
using API_Conta_SaldoExtrato.Enums;
using Prometheus;
using API_Conta_SaldoExtrato.Config;
using API_Conta_SaldoExtrato.Services;
using API_Conta_SaldoExtrato.Exceptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Diagnostics.CodeAnalysis;
using API_Conta_SaldoExtrato.Dto.Error;
using Azure.Core.Pipeline;
using Azure.Core;
using System.Net;
using Microsoft.Extensions.Hosting;

// Cria��o do builder da aplica��o web
var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
var jsonSerializerOptions = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };
// Adiciona health checks para monitoramento de sa�de da aplica��o
builder.Services.AddHealthChecks();

// Configura��es de timeout e servi�os externos via appsettings.json
builder.Services.Configure<TimeoutConfig>(builder.Configuration.GetSection("TimeoutConfig"));
builder.Services.Configure<ServicesConfig>(builder.Configuration.GetSection("ServicesConfig"));
builder.Services.Configure<CacheConfig>(builder.Configuration.GetSection("CacheConfig"));
// Obt�m a URL do Key Vault do Azure para buscar segredos
var keyVaultUrl = builder.Configuration.GetValue<string>("UrlKeyVault");

SecretClient secretClient = new SecretClient(new Uri(keyVaultUrl!), new DefaultAzureCredential());

// Adiciona o Application Insights para telemetria
builder.Services.AddApplicationInsightsTelemetry();

// Busca segredos necess�rios para a aplica��o
var auditoriaConnectionString = await secretClient.GetSecretAsync(builder.Configuration.GetValue<string>("Auditoria:EhTrilhaAuditoriaSender"));
var redisEstadoConnectionString = await secretClient.GetSecretAsync(builder.Configuration.GetValue<string>("ServicesConfig:RedisEstadoConnectionString"));
var apikey = await secretClient.GetSecretAsync(builder.Configuration.GetValue<string>("ServicesConfig:ApiKey"));

builder.Services.PostConfigure<CacheConfig>(config =>
{
    config.Connection = redisEstadoConnectionString.Value.Value;
});

// Registra o cliente de segredos como singleton
builder.Services.AddSingleton(secretClient);

// Configura o servi�o de auditoria com dados do Key Vault e appsettings

builder.Services.AddAuditoriaService(
    auditoriaConnectionString.Value.Value,
    builder.Configuration.GetValue<string>("Auditoria:EventHubName"),
    builder.Configuration.GetValue<string>("Auditoria:NomeAplicacao")
);


// Registra os servi�os de API e saldo para inje��o de depend�ncia
builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<ISaldoService, SaldoService>();
builder.Services.AddScoped<IExtratoService, ExtratoService>();
builder.Services.AddSingleton<RedisConnectionHelper, RedisConnectionHelper>();
// Configura��o do Swagger para documenta��o da API
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "API BFF Template",
        Version = "v1"
    });
});



// Adiciona suporte a controllers
builder.Services.AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Mantém os nomes exatos das propriedades
        options.JsonSerializerOptions.AllowTrailingCommas = true;
    });

// Adiciona suporte � explora��o de endpoints para Swagger
builder.Services.AddEndpointsApiExplorer();

// Propaga o header Authorization nas requisi��es HTTP
builder.Services.AddHeaderPropagation(options =>
{
    options.Headers.Add("Authorization");
});

// Configura o HttpClient para chamadas autenticadas e auditoria
builder.Services.AddHttpClient(NomeHttpClient.ApisComAuditoria.ToString(), httpClient =>
{
    httpClient.DefaultRequestHeaders.Add("ApiKey", apikey.Value.Value);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    ClientCertificateOptions = ClientCertificateOption.Manual,
    // Aceita qualquer certificado de servidor (n�o recomendado para produ��o)
    ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) =>
    {
        return true;
    }
})
.AddHttpMessageHandler<ApisComAuditoriaHandler>() // Handler customizado para auditoria
.AddHeaderPropagation(); // Propaga headers

// Adicionando autenticação
//TODO: verificar se posso rancar fora token internet e token intranet
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =  JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => {
    var lsKeys = new List<SecurityKey>();
    var publicJwkSiper = new JsonWebKey
    {
        KeyId = config["Tkn:keySiper:kid"],
        Alg = config["Tkn:keySiper:alg"],
        E = config["Tkn:keySiper:e"],
        N = config["Tkn:keySiper:n"],
        Kty = config["Tkn:keySiper:kty"],
        Use = config["Tkn:keySiper:use"]
    };
    lsKeys.Add(publicJwkSiper);
    var publicJwkCaixaTem = new JsonWebKey
    {
        KeyId = config["Tkn:keyCaixaTem:kid"],
        Alg = config["Tkn:keyCaixaTem:alg"],
        E = config["Tkn:keyCaixaTem:e"],
        N = config["Tkn:keyCaixaTem:n"],
        Kty = config["Tkn:keyCaixaTem:kty"],
        Use = config["Tkn:keyCaixaTem:use"]
    };
    lsKeys.Add(publicJwkCaixaTem);
        
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuers = config.GetSection("Tkn:issuers").AsEnumerable().Select(q => q.Value),
        ValidateAudience = true,
        ValidAudiences = config.GetSection("Tkn:auds").AsEnumerable().Select(q => q.Value),
        ValidateLifetime = true,
        ValidateIssuerSigningKey = false,
        RequireExpirationTime = true,
        IssuerSigningKeys = lsKeys,
        ClockSkew = TimeSpan.FromSeconds(20)
    };

    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context => { return Task.CompletedTask; },
        OnAuthenticationFailed = context =>
        {
            var mensagem = "Token inválido";
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Append("Token-Expired", "true");
                mensagem = "Token expirado";
            }

            var error = new Erro { Mensagem = mensagem };
            context.Response.StatusCode = 401;
            context.Response.ContentType = "application/json";
            context.Response.WriteAsync(Encoding.UTF8.GetString(JsonSerializer.SerializeToUtf8Bytes(error, jsonSerializerOptions)));

            return Task.CompletedTask;
        },
        OnChallenge = context => { return Task.CompletedTask; },
        OnMessageReceived = context => { return Task.CompletedTask; }
    };
});

// Cria a aplica��o web
var app = builder.Build();

// In Startup.cs Configure method or Program.cs
app.UsePathBase("/conta-saldoextrato"); // Sets the application's base URL path

// Mapeia endpoint de health check
app.MapHealthChecks("/healthz");
// Middleware de seguran�a requisi��es
app.UseMiddleware<SegurancaMiddleware>();

// Middleware de auditoria para registrar requisi��es
//app.UseMiddleware<AuditoriaMiddleware>();

// Middleware para m�tricas Prometheus
app.UseMetricServer();

// Middleware para tratamento customizado de exce��es
app.UseCustomExceptionHandler();

// Ativa o Swagger para documenta��o da API
app.UseSwagger(options =>
{
    //options.RouteTemplate = "{documentName}/swagger.json";
});
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "API-Conta-SaldoExtrato v1");
    options.RoutePrefix = "swagger/v1";
});

// Redireciona requisi��es HTTP para HTTPS
app.UseHttpsRedirection();

// Propaga headers configurados
app.UseHeaderPropagation();

// Middleware de autoriza��o
app.UseAuthorization();

// Mapeia os controllers da API
app.MapControllers();

// Inicia a aplica��o
app.Run();

[ExcludeFromCodeCoverage]
public partial class Program
{
    protected Program() { }
}
