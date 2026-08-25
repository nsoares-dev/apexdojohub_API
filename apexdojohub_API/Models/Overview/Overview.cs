namespace apexdojohub_API.Models.Overview
{
    public class DashboardOverview
    {
        public DashboardCards Cards { get; set; }
        public List<GraficoMes> Grafico { get; set; }
        public List<TopLancamento> MaioresDespesas { get; set; }
        public List<TopLancamento> MaioresReceitas { get; set; }
    }

    public class DashboardCards
    {
        public decimal SaldoAcumulado { get; set; }
        public DateTime? UltimaAtualizacao { get; set; }
        public decimal RecebidoAno { get; set; }
        public decimal PagoAno { get; set; }
        public decimal ResultadoAno { get; set; }
        public int MesReferencia { get; set; }
        public decimal EntrouMes { get; set; }
        public decimal SaiuMes { get; set; }
        public decimal ResultadoMes { get; set; }
    }

    public class GraficoMes
    {
        public string MesNome { get; set; }
        public decimal Entradas { get; set; }
        public decimal Saidas { get; set; }
    }

    public class TopLancamento
    {
        public string Observacao { get; set; }
        public decimal Valor { get; set; }
    }
}
