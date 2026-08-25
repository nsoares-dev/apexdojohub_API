using Microsoft.AspNetCore.Mvc;
using apexdojohub_API.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace apexdojohub_API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("API/Sincronizar")]
    public class SincronizacaoController : Controller
    {
        private readonly SyncService _syncService;

        public SincronizacaoController(SyncService syncService)
        {
            _syncService = syncService;
        }

        [HttpPost]
        [Route("rodar-sync")]
        public async Task<IActionResult> Sincronizar()
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var caminhoNoPc = @"D:\Controle Financeiro Studio versão abril-24 HBTT 10-02 - Copiar.xlsx";
                var resultado = await _syncService.SincronizarPlanilhaAsync(caminhoNoPc, usuarioId);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Mensagem = "Ocorreu um erro durante a sincronização da planilha.",
                    Detalhes = ex.Message
                });
            }
        }
    }
}
