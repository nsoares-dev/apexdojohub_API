using apexdojohub_API.Helpers;
using apexdojohub_API.Models.Mensalidades;
using Microsoft.Data.SqlClient;
using System.Data;

namespace apexdojohub_API.Services
{
    public class MensalidadeService
    {
        private readonly IDbConnection _connection;

        public MensalidadeService(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<MensalidadeDashboardResponse> ObterDashboardMensalidadesAsync(int mes, int ano, string status = "ativos", string busca = "")
        {
            try
            {
                var dashboard = new MensalidadeDashboardResponse
                {
                    Resumo = new ResumoMensalidade(),
                    Alunos = new List<AlunoMensalidade>()
                };

                using var command = _connection.CreateStoredProcedure(Constantes.Constantes.ResumoMensalidades);

                command.Parameters.Add(new SqlParameter("@Mes", mes));
                command.Parameters.Add(new SqlParameter("@Ano", ano));
                command.Parameters.Add(new SqlParameter("@Status", status));
                command.Parameters.Add(new SqlParameter("@Busca", string.IsNullOrEmpty(busca) ? DBNull.Value : busca));

                if (_connection.State != ConnectionState.Open)
                    await ((SqlConnection)_connection).OpenAsync();

                using var reader = await command.ExecuteReaderAsync();

                // 1º RESULT SET: Lendo a linha única do Resumo (Cards)
                if (await reader.ReadAsync())
                {
                    dashboard.Resumo = new ResumoMensalidade
                    {
                        TotalAtivos = reader["TotalAlunos"] != DBNull.Value ? Convert.ToInt32(reader["TotalAlunos"]) : 0,
                        Pagaram = reader["Pagaram"] != DBNull.Value ? Convert.ToInt32(reader["Pagaram"]) : 0,
                        Pendentes = reader["Pendentes"] != DBNull.Value ? Convert.ToInt32(reader["Pendentes"]) : 0,
                        ValorRecebido = reader["ValorRecebido"] != DBNull.Value ? Convert.ToDecimal(reader["ValorRecebido"]) : 0,
                        ValorPrevisto = reader["ValorPrevisto"] != DBNull.Value ? Convert.ToDecimal(reader["ValorPrevisto"]) : 0
                    };
                }

                // 2º RESULT SET: Lendo as várias linhas para montar a Tabela
                if (await reader.NextResultAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        dashboard.Alunos.Add(new AlunoMensalidade
                        {
                            AlunoId = reader["AlunoId"] != DBNull.Value ? Convert.ToInt32(reader["AlunoId"]) : 0,
                            Nome = reader["Nome"].ToString(),
                            Modalidade = reader["Modalidade"].ToString(),
                            Vencimento = reader["Vencimento"] != DBNull.Value ? Convert.ToInt32(reader["Vencimento"]) : 0,
                            Mensalidade = reader["Mensalidade"] != DBNull.Value ? Convert.ToDecimal(reader["Mensalidade"]) : 0,
                            Status = reader["Status"].ToString(), // A lógica de "Pago/Pendente" agora virá pronta da Procedure!
                            Celular = reader["Celular"] != DBNull.Value ? reader["Celular"].ToString() : ""
                        });
                    }
                }

                return dashboard;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao consultar o resumo de mensalidades: " + ex.Message);
            }
        }
    }
}