using Microsoft.AspNetCore.Mvc;
using apexdojohub_API.Services;

namespace apexdojohub_API.Controllers
{
    [ApiController]
    [Route("API/Alunos")]
    public class AlunosController : Controller
    {
        private readonly AlunoService _alunosService;
        public AlunosController(AlunoService alunosService)
        {
            _alunosService = alunosService;
        }
        [HttpGet]
        [Route("ConsultarAlunos")]
        public async Task<IActionResult> GetAlunos([FromQuery] string status = "ativos", [FromQuery] string modalidade = null, [FromQuery] string busca = null)
        {
            try
            {
                // Manda pro service fazer o trabalho sujo
                var response = await _alunosService.ObterAlunosAsync(status, modalidade, busca);

                // Devolve a lista montada pro React
                return Ok(response);
            }
            catch (Exception ex)
            {
                // Se estourar algo, devolve um erro 500 claro
                return StatusCode(500, new { message = "Erro ao carregar a lista de alunos.", detalhe = ex.Message });
            }
        }
    }
}
