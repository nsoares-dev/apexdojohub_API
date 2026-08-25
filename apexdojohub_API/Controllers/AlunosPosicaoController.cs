using Microsoft.AspNetCore.Mvc;
using apexdojohub_API.Services;

namespace apexdojohub_API.Controllers
{
    [ApiController]
    [Route("API/AlunosPosicao")]
    public class AlunosPosicaoController : Controller
    {
        private readonly AlunosPosicaoService _alunosPosicaoService;
        public AlunosPosicaoController(AlunosPosicaoService alunosPosicaoService)
        {
            _alunosPosicaoService = alunosPosicaoService;
        }
        [HttpGet]
        [Route("ConsultarAlunosPosicao")]
        public async Task<IActionResult> GetAlunosPosicao([FromQuery] int? ano, [FromQuery] string status = "todos", [FromQuery] string modalidade = null, [FromQuery] string plano = null, [FromQuery] string busca = null)
        {
            try
            {
                int anoReferencia = ano ?? DateTime.Now.Year;
                var resultado = await _alunosPosicaoService.ObterAlunosPosicaoAsync(anoReferencia, status, modalidade, busca, plano);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensagem = "Erro interno ao processar os dados da posição dos alunos.",
                    detalhe = ex.Message
                });
            }
        }
    }
}
