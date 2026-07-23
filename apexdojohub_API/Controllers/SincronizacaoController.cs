using Microsoft.AspNetCore.Mvc;
using apexdojohub_API.Services;

namespace apexdojohub_API.Controllers
{
    [ApiController]
    [Route("API/Sincronizar")]
    public class SincronizacaoController : Controller
    {
        private readonly SyncService _syncService;

        // Injeção de dependência do nosso serviço
        public SincronizacaoController(SyncService syncService)
        {
            _syncService = syncService;
        }

        [HttpPost("rodar-sync")]
        public async Task<IActionResult> Sincronizar()
        {
            try
            {

                var caminhoNoPc = @"C:\Users\nicol\source\repos\apexdojohub_API\controle_financeiro_adh.xlsx";
                // Como o link do OneDrive já está dentro do serviço, 
                // não precisamos passar nenhum ID por aqui mais. Apenas damos a ordem de execução.
                var resultado = await _syncService.SincronizarPlanilhaAsync(caminhoNoPc);

                // Devolve para o front-end aquele objeto que criamos com os contadores:
                // { "Status": "Sucesso", "AlunosAtualizados": 15, "LancamentosAtualizados": 42... }
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                // Se der qualquer erro (link do OneDrive quebrado, erro no SQL, etc),
                // a API captura e devolve um erro 500 bonito, sem quebrar o servidor.
                return StatusCode(500, new
                {
                    Mensagem = "Ocorreu um erro durante a sincronização da planilha.",
                    Detalhes = ex.Message
                });
            }
        }
    }
}
