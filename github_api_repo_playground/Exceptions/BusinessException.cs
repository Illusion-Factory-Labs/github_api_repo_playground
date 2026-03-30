using System.Diagnostics.CodeAnalysis;
using API_Conta_SaldoExtrato.DTO;

namespace API_Conta_SaldoExtrato.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class BusinessException : Exception
    {
        public ListErroDTO? erros { get; set; }

        public BusinessException(ErroDTO? erro)
        {
            erros = new ListErroDTO
            {
                Erros = new List<ErroDTO>
                {
                    new ErroDTO {
                        Codigo = erro?.Codigo,
                        Mensagem = erro?.Mensagem,
                        Orientacao = erro?.Orientacao,
                        IcNaoReenviar = erro?.IcNaoReenviar
                    }
                }
            };
        }

        public BusinessException(ListErroDTO? errors)
        {
            erros = errors;
        }

        public BusinessException(string? codigo, string? mensagem)
        {
            erros = new ListErroDTO
            {
                Erros = new List<ErroDTO>
                {
                    new ErroDTO {
                        Codigo = codigo,
                        Mensagem = mensagem
                    }
                }
            };
        }

        public static BusinessException Throw(string? codigo, string? mensagem)
        {
            return new BusinessException(new ListErroDTO
            {
                Erros = new List<ErroDTO>
                {
                    new() {
                        Codigo = codigo,
                        Mensagem = mensagem
                    }
                }
            });
        }
    }
}
