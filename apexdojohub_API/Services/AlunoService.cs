using System.Data;
using apexdojohub_API.Models.Alunos;
using apexdojohub_API.Helpers;
using Microsoft.Data.SqlClient;

namespace apexdojohub_API.Services
{
    public class AlunoService
    {
        private readonly IDbConnection _connection;

        public AlunoService(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<List<Aluno>> ObterAlunosAsync(string status, string modalidade, string busca)
        {
            var alunos = new List<Aluno>();

            using var command = _connection.CreateStoredProcedure(Constantes.Constantes.CONSULTARALUNOS);
            command.Parameters.Add(new SqlParameter("@Status", status ?? "ativos"));
            command.Parameters.Add(new SqlParameter("@Modalidade", string.IsNullOrEmpty(modalidade) ? DBNull.Value : modalidade));
            command.Parameters.Add(new SqlParameter("@Busca", string.IsNullOrEmpty(busca) ? DBNull.Value : busca));

            if (_connection.State != ConnectionState.Open)
                await ((SqlConnection)_connection).OpenAsync();

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                alunos.Add(new Aluno
                {
                    AlunoId = reader["AlunoId"] != DBNull.Value ? Convert.ToInt32(reader["AlunoId"]) : 0,
                    Nome = reader["Nome"].ToString(),
                    Modalidade = reader["Modalidade"].ToString(),
                    Graduacao = reader["Graduacao"].ToString(),
                    Plano = reader["Plano"].ToString(),
                    Mensalidade = reader["Mensalidade"] != DBNull.Value ? Convert.ToDecimal(reader["Mensalidade"]) : 0,
                    Vencimento = reader["Vencimento"] != DBNull.Value ? Convert.ToInt32(reader["Vencimento"]) : 0,
                    Celular = reader["Celular"].ToString(),
                    Status = reader["Status"].ToString(),
                    DataNascimento = reader["DataNascimento"] != DBNull.Value ? Convert.ToDateTime(reader["DataNascimento"]) : DateTime.MinValue,
                    ContatoEmergencia = reader["ContatoEmergencia"].ToString(),
                    DataMatricula = reader["Matricula"] != DBNull.Value ? Convert.ToDateTime(reader["Matricula"]) : DateTime.MinValue

                });
            }
            return alunos;
        }
    }
}
