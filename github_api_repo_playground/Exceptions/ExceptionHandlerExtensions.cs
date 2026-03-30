using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Text;
using System.Text.Json.Serialization;
using API_Conta_SaldoExtrato.DTO;
using System.Diagnostics.CodeAnalysis;

namespace API_Conta_SaldoExtrato.Exceptions
{
    /// <summary>
    /// Classe de extens�o para configurar o tratamento global de exce��es na aplica��o.
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static class ExceptionHandlerExtensions
    {
        /// <summary>
        /// M�todo de extens�o que adiciona um middleware customizado para tratamento de exce��es.
        /// </summary>
        /// <param name="app">Inst�ncia da aplica��o web</param>
        public static void UseCustomExceptionHandler(this WebApplication app)
        {
            // Configura o middleware de tratamento de exce��es
            app.UseExceptionHandler(new ExceptionHandlerOptions
            {
                ExceptionHandler = async context =>
                {
                    // Configura op��es do serializador JSON para ignorar valores nulos
                    var jsonSerializerOptions = new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

                    // Propaga o header de auditoria (X-Audit-Id) da requisi��o para a resposta
                    context.Request.Headers.TryGetValue("X-Audit-Id", out var requestId);
                    context.Response.Headers.Append("X-Audit-Id", requestId.ToString());

                    // Obt�m informa��es sobre a exce��o que ocorreu
                    var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                    string? ex = null;

                    // Trata exce��es de desserializa��o JSON
                    if (exceptionHandlerPathFeature?.Error is JsonException jsonException)
                    {
                        context.Response.StatusCode = 500;
                        context.Response.ContentType = "application/json";
                        // Em ambiente de desenvolvimento, inclui detalhes da exce��o
                        if (app.Environment.IsDevelopment())
                        {
                            ex = jsonException.ToString();
                        }
                        // Monta objeto de erro para resposta
                        var listaErros = new ListErroDTO
                        {
                            Erros = new List<ErroDTO>
                            {
                                    new ErroDTO { Mensagem = "Falha ao desserializar objeto.", Codigo = "500", Exception = ex }
                            }
                        };
                        // Escreve resposta JSON
                        await context.Response.WriteAsync(
                            Encoding.UTF8.GetString(JsonSerializer.SerializeToUtf8Bytes(listaErros, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull })));
                    }

                    // Trata exce��es de neg�cio (BusinessException)
                    if (exceptionHandlerPathFeature.Error is BusinessException)
                    {
                        // Obt�m os erros da exce��o de neg�cio
                        var businessException = exceptionHandlerPathFeature.Error as BusinessException;
                        var Erros = businessException?.erros?.Erros ?? new List<ErroDTO>();
                        context.Response.StatusCode = Erros.First().StatusCode;
                        context.Response.ContentType = "application/json";
                        // Retorna os erros no formato JSON
                        await context.Response.WriteAsync(
                            Encoding.UTF8.GetString(
                                JsonSerializer.SerializeToUtf8Bytes(
                                    Erros, jsonSerializerOptions)));
                    }
                    // Trata demais exce��es gen�ricas
                    else
                    {
                        var mensagem = exceptionHandlerPathFeature?.Error?.Message;
                        string[] lsMensagem = mensagem == null ? Array.Empty<string>() : mensagem.Split('|');
                        string codigo = "500";
                        // Se a mensagem estiver no formato "codigo|mensagem", separa os valores
                        if (lsMensagem.Length == 2)
                        {
                            codigo = lsMensagem[0];
                            mensagem = lsMensagem[1];
                        }
                        else
                        {
                            // Em ambiente de desenvolvimento, inclui detalhes da exce��o
                            if (app.Environment.IsDevelopment())
                            {
                                ex = exceptionHandlerPathFeature?.Error?.ToString();
                            }
                            mensagem = "Erro interno";
                        }
                        // Monta objeto de erro para resposta
                        var listaErros = new ListErroDTO
                        {
                            Erros = new List<ErroDTO>
                            {
                                    new ErroDTO {  Mensagem = mensagem, Codigo = codigo, Exception = ex }
                            }
                        };

                        context.Response.StatusCode = int.Parse(codigo);
                        context.Response.ContentType = "application/json";
                        // Escreve resposta JSON
                        await context.Response.WriteAsync(
                            Encoding.UTF8.GetString(
                                JsonSerializer.SerializeToUtf8Bytes(
                                    listaErros, jsonSerializerOptions)));
                    }
                },
                // Permite que respostas 404 sejam tratadas pelo handler
                AllowStatusCode404Response = true
            });
        }
    }
}
