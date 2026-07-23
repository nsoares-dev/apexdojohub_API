using Microsoft.AspNetCore.Mvc;
using apexdojohub_API.Interface;
using apexdojohub_API.Models.Usuario;
using apexdojohub_API.Services;

namespace apexdojohub_API.Controllers
{
    [ApiController]
    [Route("API/Usuario")]
    public class UsuarioController : Controller
    {
        private readonly IUsuarioInterface _usuarioService;
        private readonly TokenService _tokenService;

        public UsuarioController(IUsuarioInterface usuarioService, TokenService tokenService)
        {
            _usuarioService = usuarioService;
            _tokenService = tokenService;
        }
        [HttpPost]
        [Route("CriarUsuario")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CriarUsuario([FromForm] UsuarioPost usuario)
        {
            try
            {
                if (usuario == null)
                    return BadRequest("Dados do usuário não fornecidos.");

                await _usuarioService.CriarUsuario(usuario);
                return StatusCode(201, new
                {
                    mensagem = "Usuário criado com sucesso."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");

            }
        }

        [HttpGet]
        [Route("ConsultarUsuario/{usuarioId}")]
        public async Task<IActionResult> ConsultarUsuario(int usuarioId)
        {
            try
            {

                if (usuarioId == 0)
                    return BadRequest("usuarioId não fornecido.");

                var usuarios = await _usuarioService.ConsultarUsuario(usuarioId);

                if (usuarios == null || usuarios.Count == 0)
                    return NotFound(new { mensagem = "Usuário não encontrado." });

                return Ok(usuarios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] UsuarioLogin usuario)
        {
            try
            {
                if (usuario == null
                    || string.IsNullOrEmpty(usuario.LoginOuEmail)
                    || string.IsNullOrEmpty(usuario.Senha))
                    return BadRequest("Login e senha são obrigatórios.");

                var usuarios = await _usuarioService.LoginUsuario(usuario.LoginOuEmail);

                if (usuarios == null)
                    return Unauthorized("Login ou senha inválidos.");

                var senhaValida = BCrypt.Net.BCrypt.Verify(usuario.Senha, usuarios.Senha);

                if (!senhaValida)
                    return Unauthorized("Login ou senha inválidos.");

                var token = _tokenService.GerarToken(usuarios.UsuarioId, usuarios.Login);

                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true, // Bloqueia a leitura via JavaScript (Anti-XSS)
                    Secure = true,   // Exige HTTPS. (Aviso: mude para 'false' se estiver testando em localhost com HTTP)
                    SameSite = SameSiteMode.Lax, // Evita envio do cookie por outros sites (Anti-CSRF)
                    Expires = DateTime.UtcNow.AddHours(1) // Deve ser igual ao tempo de expiração do seu token
                };

                Response.Cookies.Append("MeuTokenSeguro", token, cookieOptions);

                var usuarioResponse = new UsuarioLoginResponseSeguro
                {
                    UsuarioId = usuarios.UsuarioId,
                    Nome = usuarios.Nome,
                    Login = usuarios.Login,
                    Email = usuarios.Email,
                    TipoUsuario = usuarios.TipoUsuario,
                    FotoPerfil = usuarios.FotoPerfil
                };

                return Ok(new
                {
                    mensagem = "Login realizado com sucesso.",
                    //token = token,
                    usuario = usuarioResponse
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno do servidor: {ex.Message}");
            }
        }

    }
}