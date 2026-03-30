using System.Diagnostics.CodeAnalysis;
using API_Conta_SaldoExtrato.DTO;

namespace API_Conta_SaldoExtrato.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class HttpCustomException : Exception
    {
        public ListErroDTO Erros { get; }

        public HttpCustomException(ErroDTO erro)
        {
            Erros = new ListErroDTO
            {
                Erros = new List<ErroDTO>
                {
                    new ErroDTO
                    {
                        Codigo = erro.Codigo,
                        Mensagem = erro.Mensagem
                    }
                }
            };
        }

        public HttpCustomException(List<ErroDTO> erros)
        {
            Erros = new ListErroDTO
            {
                Erros = erros
            };
        }
    }
}
