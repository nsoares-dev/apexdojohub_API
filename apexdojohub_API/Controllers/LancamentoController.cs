using Microsoft.AspNetCore.Mvc;
using apexdojohub_API.Services;

namespace apexdojohub_API.Controllers
{
    [ApiController]
    [Route("API/Lancamento")]
    public class LancamentoController : Controller
    {
        private readonly LancamentoService _lancamentoService;

        public LancamentoController(LancamentoService lancamentoService)
        {
            _lancamentoService = lancamentoService;
        }

        [HttpGet]
        public async Task<IActionResult> GetLancamentos([FromQuery] string? busca, [FromQuery] int? ano, [FromQuery] int? mes, [FromQuery] string? tipo, [FromQuery] string? banco)
        {
            try
            {
                var resultado = await _lancamentoService.ConsultarLancamentos(busca, ano, mes, tipo, banco);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensagem = "Erro ao buscar lançamentos.", detalhe = ex.Message });
            }
        }
    }
}
