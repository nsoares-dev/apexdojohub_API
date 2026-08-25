namespace apexdojohub_API.Models.Alunos
{
    public class Aluno
    {
        public int AlunoId { get; set; }
        public string Nome { get; set; }
        public string Modalidade { get; set; }
        public string Graduacao { get; set; }
        public string Plano { get; set; }
        public decimal Mensalidade { get; set; }
        public int Vencimento { get; set; }
        public string Celular { get; set; }
        public string Status { get; set; }
        public DateTime DataNascimento { get; set; }
        public string ContatoEmergencia { get; set; }
        public DateTime DataMatricula { get; set; }
    }
    public class PosicaoAluno
    {
        public int AlunoId { get; set; }
        public string Nome { get; set; }
        public string Modalidade { get; set; }
        public int Vencimento { get; set; }
        public string Status { get; set; }
        public string Plano { get; set; }
        public string AcertoAnterior { get; set; }
        public string Janeiro { get; set; }
        public string Fevereiro { get; set; }
        public string Marco { get; set; }
        public string Abril { get; set; }
        public string Maio { get; set; }
        public string Junho { get; set; }
        public string Julho { get; set; }
        public string Agosto { get; set; }
        public string Setembro { get; set; }
        public string Outubro { get; set; }
        public string Novembro { get; set; }
        public string Dezembro { get; set; }
        public decimal Total { get; set; }
    }

}
