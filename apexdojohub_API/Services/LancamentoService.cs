using apexdojohub_API.Helpers;
using Microsoft.Data.SqlClient;
using System.Data;
using apexdojohub_API.Models.Lancamentos;

namespace apexdojohub_API.Services
{
    public class LancamentoService
    {
        private readonly IDbConnection _connection;
        public LancamentoService(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<List<Lancamento>> ConsultarLancamentos(string? busca, int? ano, int? mes, string? tipo, string? banco)
        {
            try
            {
                var lancamentos = new List<Lancamento>();

                using var command = _connection.CreateStoredProcedure(Constantes.Constantes.Lancamentos);

                // Passando os parâmetros (Convertendo null para DBNull.Value quando necessário)
                command.Parameters.Add(new SqlParameter("@Busca", string.IsNullOrEmpty(busca) ? DBNull.Value : busca));
                command.Parameters.Add(new SqlParameter("@Ano", ano.HasValue ? (object)ano.Value : DBNull.Value));
                command.Parameters.Add(new SqlParameter("@Mes", mes.HasValue ? (object)mes.Value : DBNull.Value));
                command.Parameters.Add(new SqlParameter("@Tipo", string.IsNullOrEmpty(tipo) ? DBNull.Value : tipo));
                command.Parameters.Add(new SqlParameter("@Banco", string.IsNullOrEmpty(banco) ? DBNull.Value : banco));

                if (_connection.State != ConnectionState.Open)
                    await ((SqlConnection)_connection).OpenAsync();

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    lancamentos.Add(new Lancamento
                    {
                        Id = Convert.ToInt32(reader["Id"]),
                        DataEfetiva = Convert.ToDateTime(reader["DataEfetiva"]),
                        PagadorEstabelecimento = reader["PagadorEstabelecimento"] != DBNull.Value ? reader["PagadorEstabelecimento"].ToString() : null,
                        Observacao = reader["Observacao"] != DBNull.Value ? reader["Observacao"].ToString() : null,
                        Valor = Convert.ToDecimal(reader["Valor"]),
                        Situacao = reader["Situacao"].ToString(),
                        Banco = reader["Banco"] != DBNull.Value ? reader["Banco"].ToString() : null,
                        TipoMovimentacao = reader["TipoMovimentacao"] != DBNull.Value ? reader["TipoMovimentacao"].ToString() : null,
                        Aluno = reader["Aluno"] != DBNull.Value ? reader["Aluno"].ToString() : null,

                    });
                }
                return lancamentos;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao consultar lançamentos: " + ex.Message);
            }
        }
    }
}
