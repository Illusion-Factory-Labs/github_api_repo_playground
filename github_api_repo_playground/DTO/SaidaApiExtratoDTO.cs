using System.Diagnostics.CodeAnalysis;

namespace API_Conta_SaldoExtrato.DTO
{
    [ExcludeFromCodeCoverage]
    public class SaidaApiExtratoDTO
    {
        public string? conta { get; set; }
        public DadosCadastraisDTO? dados_cadastrais { get; set; }
        public ContaMigradaDTO? conta_migrada { get; set; }
        public List<LancamentoDTO>? lancamentos { get; set; }
        public List<CompraDTO>? compras { get; set; }
        public List<AgendamentoDTO>? agendamentos { get; set; }
        public List<LancamentoDiaDTO>? lancamentos_dia { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class DadosCadastraisDTO
    {
        public string? nome_cliente { get; set; }
        public long cpf { get; set; }
        public long cnpj { get; set; }
        public string? nome_unidade { get; set; }
        public string? descricao_produto { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class ContaMigradaDTO
    {
        public int agencia { get; set; }
        public int produto { get; set; }
        public int propriedade { get; set; }
        public int numero { get; set; }
        public string? mensagem_migracao { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class LancamentoDTO
    {
        public string? data { get; set; }
        public int numero_documento { get; set; }
        public string? descricao { get; set; }
        public decimal valor { get; set; }
        public string? sinal { get; set; }
        public decimal saldo_pos_lancamento { get; set; }
        public string? sinal_saldo { get; set; }
        public decimal taxa_produto { get; set; }

        public InformacoesAdicionaisDTO? informacoes_adicionais { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class CompraDTO
    {
        public string? data { get; set; }
        public string? hora { get; set; }
        public int numero_documento { get; set; }
        public string? descricao { get; set; }
        public decimal valor { get; set; }
        public string? sinal { get; set; }
        public string? estabelecimento { get; set; }
    }

    [ExcludeFromCodeCoverage]
    public class AgendamentoDTO
    {
        public string? data { get; set; }
        public int numero_documento { get; set; }
        public string? descricao { get; set; }
        public decimal valor { get; set; }
        public string? sinal { get; set; }
    }
        
    /// <summary>
    /// DTO para lançamentos bancários com informações adicionais
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class LancamentoResponseDTO
    {
        /// <summary>
        /// Data do lançamento no formato YYYY-MM-DD
        /// </summary>
        public string? data { get; set; }

        /// <summary>
        /// Número do documento
        /// </summary>
        public int numero_documento { get; set; }

        /// <summary>
        /// Descrição da transação
        /// </summary>
        public string? descricao { get; set; }

        /// <summary>
        /// Valor da transação
        /// </summary>
        public decimal valor { get; set; }

        /// <summary>
        /// Sinal da transação (C = Crédito, D = Débito)
        /// </summary>
        public string? sinal { get; set; }

        /// <summary>
        /// Saldo após o lançamento
        /// </summary>
        public decimal saldo_pos_lancamento { get; set; }

        /// <summary>
        /// Sinal do saldo (C = Crédito, D = Débito)
        /// </summary>
        public string? sinal_saldo { get; set; }

        /// <summary>
        /// Informações adicionais da transação
        /// </summary>
        public InformacoesAdicionaisDTO? informacoes_adicionais { get; set; }

        /// <summary>
        /// Taxa do produto
        /// </summary>
        public decimal taxa_produto { get; set; }
    }

    /// <summary>
    /// DTO para lançamentos do dia
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class LancamentoDiaDTO
    {
        /// <summary>
        /// Data do lançamento no formato YYYY-MM-DD
        /// </summary>
        public string? data { get; set; }

        /// <summary>
        /// Número do documento
        /// </summary>
        public int numero_documento { get; set; }

        /// <summary>
        /// Descrição da transação
        /// </summary>
        public string? descricao { get; set; }

        /// <summary>
        /// Valor da transação
        /// </summary>
        public decimal valor { get; set; }

        /// <summary>
        /// Sinal da transação (C = Crédito, D = Débito)
        /// </summary>
        public string? sinal { get; set; }

        /// <summary>
        /// Saldo após o lançamento
        /// </summary>
        public decimal saldo_pos_lancamento { get; set; }

        /// <summary>
        /// Sinal do saldo (C = Crédito, D = Débito)
        /// </summary>
        public string? sinal_saldo { get; set; }

        /// <summary>
        /// Informações adicionais da transação
        /// </summary>
        public InformacoesAdicionaisDTO? informacoes_adicionais { get; set; }
    }

    /// <summary>
    /// DTO para informações adicionais das transações
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class InformacoesAdicionaisDTO
    {
        /// <summary>
        /// Data e hora de efetivação da transação
        /// </summary>
        public string? data_hora_efetivacao { get; set; }

        /// <summary>
        /// Complemento da transação
        /// </summary>
        public string? complemento { get; set; }

        /// <summary>
        /// Dados da contraparte (quando aplicável)
        /// </summary>
        public ContraparteDTO? contraparte { get; set; }
    }

    /// <summary>
    /// DTO para dados da contraparte
    /// </summary>
    [ExcludeFromCodeCoverage]
    public class ContraparteDTO
    {
        /// <summary>
        /// CPF da contraparte
        /// </summary>
        public string? cpf { get; set; }

        /// <summary>
        /// Nome da contraparte
        /// </summary>
        public string? nome { get; set; }
    }
}
