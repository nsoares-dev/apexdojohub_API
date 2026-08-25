using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using apexdojohub_API.Services;

namespace apexdojohub_API.Controllers
{
    [ApiController]
    [Route("API/Overview")]
    public class OverviewController : Controller
    {
        private readonly OverviewService _overviewService;

        public OverviewController(OverviewService overviewService)
        {
            _overviewService = overviewService;
        }

        [HttpGet]
        [Route("resumo")]
        public async Task<IActionResult> GetResumoDashboard([FromQuery] int? ano, [FromQuery] int? mes)
        {
            try
            {
                int anoReferencia = ano ?? DateTime.Now.Year;

                var resultado = await _overviewService.ObterVisaoGeralAsync(anoReferencia, mes);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensagem = "Erro interno ao processar os dados do dashboard.",
                    detalhe = ex.Message
                });
            }

        }
    }
}
