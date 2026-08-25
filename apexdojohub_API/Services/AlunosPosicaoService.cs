using System.Data;
using apexdojohub_API.Models.Alunos;
using apexdojohub_API.Helpers;
using Microsoft.Data.SqlClient;

namespace apexdojohub_API.Services
{
    public class AlunosPosicaoService
    {
        private readonly IDbConnection _connection;
        public AlunosPosicaoService(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<List<PosicaoAluno>> ObterAlunosPosicaoAsync(int ano, string status, string modalidade, string busca, string plano)
        {
            var alunosPosicao = new List<PosicaoAluno>();
            using var command = _connection.CreateStoredProcedure(Constantes.Constantes.CONSULTARALUNOSPOSICAO);

            command.Parameters.Add(new SqlParameter("@Ano", ano));
            command.Parameters.Add(new SqlParameter("@Status", status ?? "ativos"));
            command.Parameters.Add(new SqlParameter("@Modalidade", string.IsNullOrEmpty(modalidade) ? DBNull.Value : modalidade));
            command.Parameters.Add(new SqlParameter("@Busca", string.IsNullOrEmpty(busca) ? DBNull.Value : busca));
            command.Parameters.Add(new SqlParameter("@Plano", string.IsNullOrEmpty(plano) ? DBNull.Value : plano));
            if (_connection.State != ConnectionState.Open)
                await ((SqlConnection)_connection).OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                alunosPosicao.Add(new PosicaoAluno
                {
                    AlunoId = reader["AlunoId"] != DBNull.Value ? Convert.ToInt32(reader["AlunoId"]) : 0,
                    Nome = reader["Nome"].ToString(),
                    Modalidade = reader["Modalidade"].ToString(),
                    Vencimento = reader["Vencimento"] != DBNull.Value ? Convert.ToInt32(reader["Vencimento"]) : 0,
                    Status = reader["Status"].ToString(),
                    Plano = reader["Plano"].ToString(),
                    AcertoAnterior = reader["AcertoAnoAnterior"].ToString(),
                    Janeiro = reader["Janeiro"].ToString(),
                    Fevereiro = reader["Fevereiro"].ToString(),
                    Marco = reader["Marco"].ToString(),
                    Abril = reader["Abril"].ToString(),
                    Maio = reader["Maio"].ToString(),
                    Junho = reader["Junho"].ToString(),
                    Julho = reader["Julho"].ToString(),
                    Agosto = reader["Agosto"].ToString(),
                    Setembro = reader["Setembro"].ToString(),
                    Outubro = reader["Outubro"].ToString(),
                    Novembro = reader["Novembro"].ToString(),
                    Dezembro = reader["Dezembro"].ToString(),
                    Total = reader["Total"] != DBNull.Value ? Convert.ToDecimal(reader["Total"]) : 0
                });
            }
            return alunosPosicao;
        }
    }
}
