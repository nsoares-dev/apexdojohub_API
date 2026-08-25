using apexdojohub_API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace apexdojohub_API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("API/Log")]
    public class LogController : Controller
    {
        private readonly LogSyncService _logSyncService;

        public LogController(LogSyncService logSyncService)
        {
            _logSyncService = logSyncService;
        }
        [HttpGet]
        public async Task<IActionResult> GetLogSincronizacao()
        {
            try
            {
                var usuarioId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var logs = await _logSyncService.ObterLogSincronizacao();
                return Ok(logs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Mensagem = "Ocorreu um erro ao obter o log de sincronização.",
                    Detalhes = ex.Message
                });
            }
        }
    }
}
