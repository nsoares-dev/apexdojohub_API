using apexdojohub_API.Helpers;
using apexdojohub_API.Models.Log;
using Microsoft.Data.SqlClient;
using System.Data;

namespace apexdojohub_API.Services
{
    public class LogSyncService
    {
        private readonly IDbConnection _connection;

        public LogSyncService(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<List<LogSync>> ObterLogSincronizacao()
        {
            var logs = new List<LogSync>();
            using var command = _connection.CreateStoredProcedure(Constantes.Constantes.SYNC_LOG);

            if (_connection.State != ConnectionState.Open)
                await ((SqlConnection)_connection).OpenAsync();

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                logs.Add(new LogSync
                {
                    Id = reader.GetInt32("Id"),
                    DataSincronizacao = reader.GetDateTime("DataSincronizacao"),
                    NomeUsuario = reader.GetString("NomeUsuario"),
                    Alunos = reader.GetBoolean("Alunos"),
                    AlunosAtualizados = reader.GetInt32("AlunosAtualizados"),
                    FluxoCaixa = reader.GetBoolean("FluxoCaixa"),
                    FluxoCaixaAtualizados = reader.GetInt32("FluxoCaixaAtualizados"),
                    PosicaoAlunos = reader.GetBoolean("PosicaoAlunos"),
                    PosicaoAlunosAtualizados = reader.GetInt32("PosicaoAlunosAtualizados")
                });
            }
            return logs;
        }
    }
}
