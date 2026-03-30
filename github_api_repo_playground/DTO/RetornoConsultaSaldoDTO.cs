namespace API_Conta_SaldoExtrato.DTO
{
    public class RetornoConsultaSaldoDTO
    {
        public decimal saldo { get; set; }
        public decimal limite { get; set; }
        public decimal saldoBloqueado { get; set; }
        public decimal saldoTotal { get; set; }
    }
}
