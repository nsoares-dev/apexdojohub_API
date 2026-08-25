using Microsoft.AspNetCore.Mvc;
using apexdojohub_API.Services;

namespace apexdojohub_API.Controllers
{
    [ApiController]
    [Route("API/Mensalidades")]
    public class MensalidadeController : Controller
    {
        private readonly MensalidadeService _mensalidadeService;

        public MensalidadeController(MensalidadeService mensalidadeService)
        {
            _mensalidadeService = mensalidadeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetMensalidades([FromQuery] int mes,  [FromQuery] int ano, [FromQuery] string status = "ativos", [FromQuery] string busca = "")
        {
            try
            {
                if (mes < 1 || mes > 12 || ano < 2000)
                {
                    return BadRequest(new { message = "Mês ou ano inválidos." });
                }

                var response = await _mensalidadeService.ObterDashboardMensalidadesAsync(mes, ano, status, busca);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Ocorreu um erro ao carregar o dashboard de mensalidades.",
                    detalhe = ex.Message
                });
            }
        }
    }
}
