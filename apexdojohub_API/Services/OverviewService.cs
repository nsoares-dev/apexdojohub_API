using apexdojohub_API.Helpers;
using Microsoft.Data.SqlClient;
using System.Data;
using apexdojohub_API.Models.Overview;

namespace apexdojohub_API.Services
{
    public class OverviewService
    {
        private readonly IDbConnection _connection;
        public OverviewService(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<DashboardOverview> ObterVisaoGeralAsync(int ano, int? mes = null)
        {
            try
            {
                var overview = new DashboardOverview
                {
                    Cards = new DashboardCards(),
                    Grafico = new List<GraficoMes>(),
                    MaioresDespesas = new List<TopLancamento>(),
                    MaioresReceitas = new List<TopLancamento>()
                };

                using var command = _connection.CreateStoredProcedure(Constantes.Constantes.ResumoDasboard);

                command.Parameters.Add(new SqlParameter("@Ano", ano));
                command.Parameters.Add(new SqlParameter("@Mes", mes.HasValue ? (object)mes.Value : DBNull.Value));

                if (_connection.State != ConnectionState.Open)
                    await ((SqlConnection)_connection).OpenAsync();

                using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    overview.Cards = new DashboardCards
                    {
                        SaldoAcumulado = reader["SaldoAcumulado"] != DBNull.Value ? Convert.ToDecimal(reader["SaldoAcumulado"]) : 0,
                        UltimaAtualizacao = reader["UltimaAtualizacao"] != DBNull.Value ? Convert.ToDateTime(reader["UltimaAtualizacao"]) : null,
                        RecebidoAno = reader["RecebidoAno"] != DBNull.Value ? Convert.ToDecimal(reader["RecebidoAno"]) : 0,
                        PagoAno = reader["PagoAno"] != DBNull.Value ? Convert.ToDecimal(reader["PagoAno"]) : 0,
                        ResultadoAno = reader["ResultadoAno"] != DBNull.Value ? Convert.ToDecimal(reader["ResultadoAno"]) : 0,
                        MesReferencia = reader["MesReferencia"] != DBNull.Value ? Convert.ToInt32(reader["MesReferencia"]) : 0,
                        EntrouMes = reader["EntrouMes"] != DBNull.Value ? Convert.ToDecimal(reader["EntrouMes"]) : 0,
                        SaiuMes = reader["SaiuMes"] != DBNull.Value ? Convert.ToDecimal(reader["SaiuMes"]) : 0,
                        ResultadoMes = reader["ResultadoMes"] != DBNull.Value ? Convert.ToDecimal(reader["ResultadoMes"]) : 0
                    };
                }

                //Gráfico Mês a Mês (Retorna 12 linhas)
                if (await reader.NextResultAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        overview.Grafico.Add(new GraficoMes
                        {
                            MesNome = reader["MesNome"].ToString(),
                            Entradas = reader["Entradas"] != DBNull.Value ? Convert.ToDecimal(reader["Entradas"]) : 0,
                            Saidas = reader["Saidas"] != DBNull.Value ? Convert.ToDecimal(reader["Saidas"]) : 0
                        });
                    }
                }

                if (await reader.NextResultAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        overview.MaioresDespesas.Add(new TopLancamento
                        {
                            Observacao = reader["PagadorEstabelecimento"].ToString(),
                            Valor = reader["Valor"] != DBNull.Value ? Convert.ToDecimal(reader["Valor"]) : 0
                        });
                    }
                }

                if (await reader.NextResultAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        overview.MaioresReceitas.Add(new TopLancamento
                        {
                            Observacao = reader["PagadorEstabelecimento"].ToString(),
                            Valor = reader["Valor"] != DBNull.Value ? Convert.ToDecimal(reader["Valor"]) : 0
                        });
                    }
                }

                return overview;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao consultar o resumo do dashboard: " + ex.Message);
            }
        }

    }
}
