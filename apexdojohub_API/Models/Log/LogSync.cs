namespace apexdojohub_API.Models.Log
{
    public class LogSync
    {
        public int Id { get; set; }
        public DateTime DataSincronizacao { get; set; }
        public string NomeUsuario { get; set; }
        public bool Alunos { get; set; }
        public int AlunosAtualizados { get; set; }
        public bool FluxoCaixa { get; set; }
        public int FluxoCaixaAtualizados { get; set; }
        public bool PosicaoAlunos { get; set; }
        public int PosicaoAlunosAtualizados { get; set; }
    }
}
