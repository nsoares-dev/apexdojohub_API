using apexdojohub_API.Helpers;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using Azure.Identity;
using Microsoft.Graph;
using System.IO;
using System.Collections.Generic;
using ExcelDataReader;
using System;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Drawing;

namespace apexdojohub_API.Services
{
    public class SyncService
    {
        private readonly IDbConnection _connection;

        public SyncService(IDbConnection connection)
        {
            _connection = connection;
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }

        public async Task<object> SincronizarPlanilhaAsync(string caminhoLocalDoArquivo, int usuarioId)
        {
            try
            {
                using var excelStream = new FileStream(caminhoLocalDoArquivo, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                int totalAlunos = 0, totalFluxo = 0, totalPosicaoAlunos = 0;
                bool alunos = false, fluxo = false, posicaoAlunos = false;

                if (_connection.State != ConnectionState.Open)
                    await ((SqlConnection)_connection).OpenAsync();

                using var reader = ExcelReaderFactory.CreateReader(excelStream);

                do
                {
                    if (reader.Name.Trim().Equals("Dados dos Alunos", StringComparison.OrdinalIgnoreCase))
                    {
                        for (int i = 0; i < 5; i++) reader.Read();

                        while (reader.Read())
                        {
                            var nomeStr = reader.GetValue(1)?.ToString();
                            if (string.IsNullOrWhiteSpace(nomeStr)) continue;

                            using var commandAlunos = _connection.CreateStoredProcedure(Constantes.Constantes.SYNCALUNO);

                            commandAlunos.Parameters.Add(new SqlParameter("@Nome", nomeStr));
                            commandAlunos.Parameters.Add(new SqlParameter("@Status", reader.GetValue(2)?.ToString() ?? (object)DBNull.Value));

                            var cellNasc = reader.GetValue(3);
                            commandAlunos.Parameters.Add(new SqlParameter("@Nascimento", cellNasc is DateTime ? (DateTime)cellNasc : (object)DBNull.Value));

                            commandAlunos.Parameters.Add(new SqlParameter("@Celular", reader.GetValue(4)?.ToString() ?? (object)DBNull.Value));
                            commandAlunos.Parameters.Add(new SqlParameter("@ContatoEmergencia", reader.GetValue(5)?.ToString() ?? (object)DBNull.Value));

                            var cellMatr = reader.GetValue(6);
                            commandAlunos.Parameters.Add(new SqlParameter("@Matricula", cellMatr is DateTime ? (DateTime)cellMatr : (object)DBNull.Value));

                            commandAlunos.Parameters.Add(new SqlParameter("@Modalidade", reader.GetValue(7)?.ToString() ?? (object)DBNull.Value));
                            commandAlunos.Parameters.Add(new SqlParameter("@Faixa", reader.GetValue(8)?.ToString() ?? (object)DBNull.Value));
                            commandAlunos.Parameters.Add(new SqlParameter("@Plano", reader.GetValue(9)?.ToString() ?? (object)DBNull.Value));

                            var cellMensalidade = reader.GetValue(10);
                            if (cellMensalidade != null && decimal.TryParse(cellMensalidade.ToString(), out decimal m))
                                commandAlunos.Parameters.Add(new SqlParameter("@Mensalidade", m));
                            else
                                commandAlunos.Parameters.Add(new SqlParameter("@Mensalidade", DBNull.Value));

                            await commandAlunos.ExecuteNonQueryAsync();
                            totalAlunos++;
                        }
                        alunos = true;
                    }
                    else if (reader.Name.Trim().Equals("Fluxo de Caixa", StringComparison.OrdinalIgnoreCase))
                    {
                        for (int i = 0; i < 9; i++) reader.Read();

                        await _connection.ExecuteAsync("TRUNCATE TABLE FluxoCaixa");

                        while (reader.Read())
                        {
                            var dataEfetivaRaw = reader.GetValue(0);
                            var pagador = reader.GetValue(2)?.ToString();

                            if (dataEfetivaRaw == null || string.IsNullOrWhiteSpace(pagador)) continue;

                            using var commandFluxo = _connection.CreateStoredProcedure(Constantes.Constantes.SYNC_FluxoDeCaixa);

                            commandFluxo.Parameters.Add(new SqlParameter("@DataEfetiva", dataEfetivaRaw is DateTime ? (DateTime)dataEfetivaRaw : (object)DBNull.Value));

                            commandFluxo.Parameters.Add(new SqlParameter("@MesReferencia", reader.GetValue(1)?.ToString() ?? (object)DBNull.Value));

                            commandFluxo.Parameters.Add(new SqlParameter("@Pagador", pagador));

                            commandFluxo.Parameters.Add(new SqlParameter("@Aluno", reader.GetValue(3)?.ToString() ?? (object)DBNull.Value));

                            var valorRaw = reader.GetValue(4);
                            decimal valorConvertido = 0;
                            if (valorRaw != null && decimal.TryParse(valorRaw.ToString(), out decimal v))
                                valorConvertido = v;
                            commandFluxo.Parameters.Add(new SqlParameter("@Valor", valorConvertido));

                            commandFluxo.Parameters.Add(new SqlParameter("@TipoMovimentacao", reader.GetValue(5)?.ToString() ?? (object)DBNull.Value));

                            var cellDataPag = reader.GetValue(6);
                            commandFluxo.Parameters.Add(new SqlParameter("@DataPagamento", cellDataPag is DateTime ? (DateTime)cellDataPag : (object)DBNull.Value));

                            commandFluxo.Parameters.Add(new SqlParameter("@Situacao", reader.GetValue(7)?.ToString() ?? (object)DBNull.Value));
                            commandFluxo.Parameters.Add(new SqlParameter("@Parcelas", reader.GetValue(8)?.ToString() ?? (object)DBNull.Value));
                            commandFluxo.Parameters.Add(new SqlParameter("@Banco", reader.GetValue(9)?.ToString() ?? (object)DBNull.Value));
                            commandFluxo.Parameters.Add(new SqlParameter("@FormaPagamento", reader.GetValue(10)?.ToString() ?? (object)DBNull.Value));
                            commandFluxo.Parameters.Add(new SqlParameter("@BandeiraCartao", reader.GetValue(11)?.ToString() ?? (object)DBNull.Value));
                            commandFluxo.Parameters.Add(new SqlParameter("@FinalCartao", reader.GetValue(12)?.ToString() ?? (object)DBNull.Value));
                            commandFluxo.Parameters.Add(new SqlParameter("@CategoriaTipo", reader.GetValue(13)?.ToString() ?? (object)DBNull.Value));
                            commandFluxo.Parameters.Add(new SqlParameter("@Observacao", reader.GetValue(14)?.ToString() ?? (object)DBNull.Value));

                            await commandFluxo.ExecuteNonQueryAsync();
                            totalFluxo++;
                        }
                        fluxo = true;
                    }
                    else if (reader.Name.Trim().StartsWith("Alunos Posição", StringComparison.OrdinalIgnoreCase))
                    {
                        string nomeAba = reader.Name.Trim();

                        string anoStr = nomeAba.Replace("Alunos Posição", "", StringComparison.OrdinalIgnoreCase).Trim();

                        if (int.TryParse(anoStr, out int anoReferencia))
                        {
                            int anoAtual = DateTime.Now.Year;

                            if (anoReferencia >= anoAtual)
                            {
                                await _connection.ExecuteAsync("DELETE FROM PosicaoAlunos WHERE AnoReferencia = @Ano", new { Ano = anoReferencia });

                                for (int i = 0; i < 6; i++) reader.Read();

                                while (reader.Read())
                                {
                                    var nomeAluno = reader.GetValue(0)?.ToString();
                                    if (string.IsNullOrWhiteSpace(nomeAluno)) continue;

                                    using var commandPosicao = _connection.CreateStoredProcedure(Constantes.Constantes.SYNC_AlunosPosicao);

                                    commandPosicao.Parameters.Add(new SqlParameter("@AnoReferencia", anoReferencia));
                                    commandPosicao.Parameters.Add(new SqlParameter("@NomeAluno", nomeAluno));
                                    commandPosicao.Parameters.Add(new SqlParameter("@Modalidade", reader.GetValue(1)?.ToString() ?? (object)DBNull.Value));
                                    commandPosicao.Parameters.Add(new SqlParameter("@Vencimento", reader.GetValue(2)?.ToString() ?? (object)DBNull.Value));
                                    commandPosicao.Parameters.Add(new SqlParameter("@StatusAluno", reader.GetValue(3)?.ToString() ?? (object)DBNull.Value));
                                    commandPosicao.Parameters.Add(new SqlParameter("@Plano", reader.GetValue(4)?.ToString() ?? (object)DBNull.Value));
                                    commandPosicao.Parameters.Add(new SqlParameter("@AcertoAnoAnterior", reader.GetValue(5)?.ToString() ?? (object)DBNull.Value));

                                    commandPosicao.Parameters.Add(new SqlParameter("@Jan", reader.GetValue(6)?.ToString() ?? (object)DBNull.Value));
                                    commandPosicao.Parameters.Add(new SqlParameter("@Fev", reader.GetValue(7)?.ToString() ?? (object)DBNull.Value));
                                    commandPosicao.Parameters.Add(new SqlParameter("@Mar", reader.GetValue(8)?.ToString() ?? (object)DBNull.Value));
                                    commandPosicao.Parameters.Add(new SqlParameter("@Abr", reader.GetValue(9)?.ToString() ?? (object)DBNull.Value));
                                    commandPosicao.Parameters.Add(new SqlParameter("@Mai", reader.GetValue(10)?.ToString() ?? (object)DBNull.Value));
                                    commandPosicao.Parameters.Add(new SqlParameter("@Jun", reader.GetValue(11)?.ToString() ?? (object)DBNull.Value));
                                    commandPosicao.Parameters.Add(new SqlParameter("@Jul", reader.GetValue(12)?.ToString() ?? (object)DBNull.Value));
                                    commandPosicao.Parameters.Add(new SqlParameter("@Ago", reader.GetValue(13)?.ToString() ?? (object)DBNull.Value));
                                    commandPosicao.Parameters.Add(new SqlParameter("@Setembro", reader.GetValue(14)?.ToString() ?? (object)DBNull.Value));
                                    commandPosicao.Parameters.Add(new SqlParameter("@Out", reader.GetValue(15)?.ToString() ?? (object)DBNull.Value));
                                    commandPosicao.Parameters.Add(new SqlParameter("@Nov", reader.GetValue(16)?.ToString() ?? (object)DBNull.Value));
                                    commandPosicao.Parameters.Add(new SqlParameter("@Dez", reader.GetValue(17)?.ToString() ?? (object)DBNull.Value));

                                    var valorRaw = reader.GetValue(18);
                                    decimal valorConvertido = 0;
                                    if (valorRaw != null && decimal.TryParse(valorRaw.ToString(), out decimal v))
                                        valorConvertido = v;
                                    commandPosicao.Parameters.Add(new SqlParameter("@Total", valorConvertido));

                                    await commandPosicao.ExecuteNonQueryAsync();
                                    totalPosicaoAlunos++;
                                }
                                posicaoAlunos = true;
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine($"Aba 'Alunos Posição {anoReferencia}' ignorada por ser de ano passado.");
                            }
                        }
                    }

                } while (reader.NextResult()); // vai para a próxima aba

                using var commandSyncLog = _connection.CreateStoredProcedure(Constantes.Constantes.SYNC_LOG);

                commandSyncLog.Parameters.Add(new SqlParameter("@UsuarioId", usuarioId));
                commandSyncLog.Parameters.Add(new SqlParameter("@Alunos", alunos));
                commandSyncLog.Parameters.Add(new SqlParameter("@AlunosAtualizados", totalAlunos));
                commandSyncLog.Parameters.Add(new SqlParameter("@FluxoCaixa", fluxo));
                commandSyncLog.Parameters.Add(new SqlParameter("@FluxoCaixaAtualizados", totalFluxo));
                commandSyncLog.Parameters.Add(new SqlParameter("@PosicaoAlunos", posicaoAlunos));
                commandSyncLog.Parameters.Add(new SqlParameter("@PosicaoAlunosAtualizados", totalPosicaoAlunos));

                await commandSyncLog.ExecuteNonQueryAsync();

                return new
                {
                    Status = "Sucesso",
                    AlunosAtualizados = totalAlunos,
                    LancamentosAtualizados = totalFluxo,
                    PosicaoAlunosAtualizados = totalPosicaoAlunos
                };
            }
            catch (Exception ex)
            {
                return new
                {
                    Status = "Erro",
                    Mensagem = ex.Message
                };
            }
        }

        private async Task<MemoryStream> BaixarArquivoDoOneDrive()
        {
            var linkCompartilhado = "https://1drv.ms/x/c/35d4e6024a36b4ed/IQCLfb1GlSZlToF2477SpknGAd1o2uYQe2Mjp6mO_pb54D4?e=kxWJ6q";

            string base64Value = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(linkCompartilhado))
                .Replace('/', '_')
                .Replace('+', '-')
                .TrimEnd('=');

            string encodedUrl = "u!" + base64Value;
            string linkDownloadDireto = $"https://api.onedrive.com/v1.0/shares/{encodedUrl}/root/content";

            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(linkDownloadDireto);
            response.EnsureSuccessStatusCode();

            var streamBytes = await response.Content.ReadAsByteArrayAsync();

            if (streamBytes.Length < 2 || streamBytes[0] != 80 || streamBytes[1] != 75)
            {
                var htmlRetornado = System.Text.Encoding.UTF8.GetString(streamBytes);
                throw new Exception($"A API não retornou o Excel. Retorno: {htmlRetornado.Substring(0, Math.Min(htmlRetornado.Length, 500))}...");
            }

            return new MemoryStream(streamBytes);
        }
    }
}