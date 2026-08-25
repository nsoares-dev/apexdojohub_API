namespace apexdojohub_API.Models.Lancamentos
{
    public class Lancamento
    {
        public int Id { get; set; }
        public DateTime DataEfetiva { get; set; }
        public string PagadorEstabelecimento { get; set; }
        public string Observacao { get; set; }
        public string Aluno { get; set; }
        public decimal Valor { get; set; }
        public string Situacao { get; set; }
        public string Banco { get; set; }
        public string TipoMovimentacao { get; set; }
    }
}
