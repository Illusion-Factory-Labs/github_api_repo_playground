using System.Diagnostics.CodeAnalysis;

namespace API_Conta_SaldoExtrato.DTO
{
    [ExcludeFromCodeCoverage]
    public class EntradaPadraoSuperAppDTO
    {
        //conta de origem
        public ContaDTO? contaOrigem { get; set; }

        //cpf do cliente
        public decimal? cpfCliente { get; set; }
        //contrato de Credito
        public ContratoCreditoDTO? contratoCredito { get; set; }
        //cartao
        public CartaoDTO? cartao { get; set; }

        public AssinaturaDTO? assinatura { get; set; }

    }

    [ExcludeFromCodeCoverage]
    //Assinatura eletronica
    public class AssinaturaDTO
    {
        public string assinatura { get; set; }
    }

    [ExcludeFromCodeCoverage]
    //Cartao 
    public class CartaoDTO
    {
        public decimal numeroCartao { get; set; }
        public string? modalidade { get; set; }
        public string? bandeira { get; set; }
        public short? produto { get; set; }
    }

    [ExcludeFromCodeCoverage]
    //contrato de credito
    public class ContratoCreditoDTO
    {
        public decimal numeroContrato { get; set; }
        public short? codigoCistema { get; set; }
        public string? nomeSistema { get; set; }
        public short? produto { get; set; }

    }

    [ExcludeFromCodeCoverage]
    //DTo de conta
    public class ContaDTO
    {
        public short unidade { get; set; }
        public short operacao { get; set; }
        public int conta { get; set; }
        public short dv { get; set; }
    }

}
