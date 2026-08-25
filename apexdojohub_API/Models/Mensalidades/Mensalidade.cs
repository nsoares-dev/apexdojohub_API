namespace apexdojohub_API.Models.Mensalidades
{
    public class MensalidadeDashboardResponse
    {
        public ResumoMensalidade Resumo { get; set; }
        public List<AlunoMensalidade> Alunos { get; set; }
    }

    public class ResumoMensalidade
    {
        public int TotalAtivos { get; set; }
        public int Pagaram { get; set; }
        public int Pendentes { get; set; }
        public decimal ValorRecebido { get; set; }
        public decimal ValorPrevisto { get; set; }
    }

    public class AlunoMensalidade
    {
        public int AlunoId { get; set; }
        public string Nome { get; set; }
        public string Modalidade { get; set; }
        public int Vencimento { get; set; }
        public decimal Mensalidade { get; set; }
        public string Status { get; set; }
        public string Celular { get; set; }
    }

}
