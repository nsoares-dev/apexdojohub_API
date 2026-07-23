using apexdojohub_API.Helpers;
using ClosedXML.Excel;
using Dapper;
using Microsoft.Data.SqlClient;
using System.Data;
using Azure.Identity;
using Microsoft.Graph;
using System.IO;


namespace apexdojohub_API.Services
{
    public class SyncService
    {
        private readonly IDbConnection _connection;
        public SyncService(IDbConnection connection)
        {
            _connection = connection;
        }
        //public async Task<object> SincronizarPlanilhaAsync()
        //{
        //    try
        //    {
        //        using var excelStream = await BaixarArquivoDoOneDrive();
        //        int totalAlunos = 0, totalLancamentos = 0, totalResumo = 0;

        //        if (_connection.State != ConnectionState.Open)
        //            await ((SqlConnection)_connection).OpenAsync();

        //        // processar a aba alunos
        //        using (var planilha = new XLWorkbook(excelStream))
        //        {
        //            var wsAlunos = planilha.Worksheet("Alunos");
        //            var linhasAlunos = wsAlunos.RangeUsed().RowsUsed().Skip(1); // pular 1ª Linha ou cabeçalho

        //            foreach (var row in linhasAlunos)
        //            {
        //                using var commandAlunos = _connection.CreateStoredProcedure(Constantes.Constantes.SYNCALUNO);

        //                commandAlunos.Parameters.Add(new SqlParameter("@Nome", row.Cell("A").GetValue<string>()));
        //                commandAlunos.Parameters.Add(new SqlParameter("@Status", row.Cell("B").GetValue<string>()));
        //                commandAlunos.Parameters.Add(new SqlParameter("@Mensalidade", row.Cell("J").GetValue<decimal>()));
        //                commandAlunos.Parameters.Add(new SqlParameter("@DiaVencimento", row.Cell("K").GetValue<int>()));

        //                var cellNascimento = row.Cell("C");
        //                commandAlunos.Parameters.Add(new SqlParameter("@Nascimento", cellNascimento.IsEmpty() ? DBNull.Value : cellNascimento.GetDateTime()));

        //                var cellMatricula = row.Cell("F");
        //                commandAlunos.Parameters.Add(new SqlParameter("@Matricula", cellMatricula.IsEmpty() ? DBNull.Value : cellMatricula.GetDateTime()));

        //                var cellCelular = row.Cell("D").GetValue<string>();
        //                commandAlunos.Parameters.Add(new SqlParameter("@Celular", string.IsNullOrWhiteSpace(cellCelular) ? DBNull.Value : cellCelular));

        //                var cellContato = row.Cell("E").GetValue<string>();
        //                commandAlunos.Parameters.Add(new SqlParameter("@ContatoEmergencia", string.IsNullOrWhiteSpace(cellContato) ? DBNull.Value : cellContato));

        //                commandAlunos.Parameters.Add(new SqlParameter("@Modalidade", row.Cell("G").GetValue<string>()));
        //                commandAlunos.Parameters.Add(new SqlParameter("@Faixa", row.Cell("H").GetValue<string>()));
        //                commandAlunos.Parameters.Add(new SqlParameter("@Plano", row.Cell("I").GetValue<string>()));

        //                await commandAlunos.ExecuteNonQueryAsync();
        //                totalAlunos++;
        //            }
        //            return new
        //            {
        //                Status = "Sucesso",
        //                AlunosAtualizados = totalAlunos
        //                //LancamentosAtualizados = totalLancamentos,
        //                //ResumoAtualizado = totalResumo > 0
        //            };
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new
        //        {
        //            Status = "Erro",
        //            Mensagem = ex.Message
        //        };
        //    }
        //}

        public async Task<object> SincronizarPlanilhaAsync(string caminhoLocalDoArquivo)
        {
            try
            {
                using var excelStream = new FileStream(caminhoLocalDoArquivo, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                int totalAlunos = 0, totalLancamentos = 0, totalResumo = 0;

                if (_connection.State != ConnectionState.Open)
                    await ((SqlConnection)_connection).OpenAsync();

                // processar a aba alunos
                using (var planilha = new XLWorkbook(excelStream))
                {
                    var wsAlunos = planilha.Worksheet("Alunos");
                    var linhasAlunos = wsAlunos.RangeUsed().RowsUsed().Skip(1); // pular 1ª Linha ou cabeçalho

                    foreach (var row in linhasAlunos)
                    {
                        using var commandAlunos = _connection.CreateStoredProcedure(Constantes.Constantes.SYNCALUNO);

                        commandAlunos.Parameters.Add(new SqlParameter("@Nome", row.Cell("A").GetValue<string>() ?? (object)DBNull.Value));
                        commandAlunos.Parameters.Add(new SqlParameter("@Status", row.Cell("B").GetValue<string>() ?? (object)DBNull.Value));

                        // 2. Datas (Tratamento 100% seguro com cast de object nos dois lados do IF)
                        var cellNasc = row.Cell("C");
                        commandAlunos.Parameters.Add(new SqlParameter("@Nascimento", cellNasc.IsEmpty() ? (object)DBNull.Value : (object)cellNasc.GetDateTime()));

                        var cellMatr = row.Cell("F");
                        commandAlunos.Parameters.Add(new SqlParameter("@Matricula", cellMatr.IsEmpty() ? (object)DBNull.Value : (object)cellMatr.GetDateTime()));

                        // 3. Textos Opcionais
                        var txtCelular = row.Cell("D").GetValue<string>();
                        commandAlunos.Parameters.Add(new SqlParameter("@Celular", string.IsNullOrWhiteSpace(txtCelular) ? (object)DBNull.Value : (object)txtCelular));

                        var txtContato = row.Cell("E").GetValue<string>();
                        commandAlunos.Parameters.Add(new SqlParameter("@ContatoEmergencia", string.IsNullOrWhiteSpace(txtContato) ? (object)DBNull.Value : (object)txtContato));

                        // 4. Classificações
                        commandAlunos.Parameters.Add(new SqlParameter("@Modalidade", row.Cell("G").GetValue<string>() ?? (object)DBNull.Value));
                        commandAlunos.Parameters.Add(new SqlParameter("@Faixa", row.Cell("H").GetValue<string>() ?? (object)DBNull.Value));
                        commandAlunos.Parameters.Add(new SqlParameter("@Plano", row.Cell("I").GetValue<string>() ?? (object)DBNull.Value));

                        // 5. Financeiro
                        var cellMensalidade = row.Cell("J");
                        commandAlunos.Parameters.Add(new SqlParameter("@Mensalidade", cellMensalidade.IsEmpty() ? (object)DBNull.Value : (object)cellMensalidade.GetValue<decimal>()));

                        var cellVencimento = row.Cell("K");
                        object valorVencimento = DBNull.Value;
                        if (cellVencimento.TryGetValue<int>(out int diaVencimento))
                        {
                            valorVencimento = diaVencimento;
                        }
                        commandAlunos.Parameters.Add(new SqlParameter("@DiaVencimento", valorVencimento));

                        await commandAlunos.ExecuteNonQueryAsync();
                        totalAlunos++;
                    }
                    return new
                    {
                        Status = "Sucesso",
                        AlunosAtualizados = totalAlunos
                        //LancamentosAtualizados = totalLancamentos,
                        //ResumoAtualizado = totalResumo > 0
                    };
                }
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
            // O link direto de download da sua planilha
            var linkCompartilhado = "https://1drv.ms/x/c/35d4e6024a36b4ed/IQCLfb1GlSZlToF2477SpknGAd1o2uYQe2Mjp6mO_pb54D4?e=kxWJ6q";

            // 2. A MÁGICA DA API: Converte o link para o formato URL-Safe Base64 que a Microsoft exige
            string base64Value = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(linkCompartilhado))
                .Replace('/', '_')
                .Replace('+', '-')
                .TrimEnd('=');

            string encodedUrl = "u!" + base64Value;

            // 3. Monta a URL oficial da API do OneDrive (Essa NUNCA expira e só traz o arquivo cru)
            string linkDownloadDireto = $"https://api.onedrive.com/v1.0/shares/{encodedUrl}/root/content";

            using var httpClient = new HttpClient();

            // Faz a requisição direto para a API
            var response = await httpClient.GetAsync(linkDownloadDireto);
            response.EnsureSuccessStatusCode();

            var streamBytes = await response.Content.ReadAsByteArrayAsync();

            // Mantemos a trava de segurança do Arquiteto (Verifica se começa com PK / Arquivo ZIP do Excel)
            if (streamBytes.Length < 2 || streamBytes[0] != 80 || streamBytes[1] != 75)
            {
                var htmlRetornado = System.Text.Encoding.UTF8.GetString(streamBytes);
                throw new Exception($"A API não retornou o Excel. Retorno: {htmlRetornado.Substring(0, Math.Min(htmlRetornado.Length, 500))}...");
            }

            return new MemoryStream(streamBytes);
        }


        //private async Task<MemoryStream> BaixarArquivoDoOneDrive(string fileId)
        //{
        //    // O código real da Graph API entra aqui depois
        //    return new MemoryStream();
        //}
    }
}
