using apexdojohub_API.Enum;
using System.ComponentModel.DataAnnotations;

namespace apexdojohub_API.Models.Usuario
{
    public class UsuarioPost
    {
        [Required(ErrorMessage = "O nome é obrigatório")]
        public string Nome { get; set; }
        public string Login { get; set; }
        [EmailAddress(ErrorMessage = "O campo E-mail não está preenchido corretamente.")]
        public string Email { get; set; }
        [MinLength(7, ErrorMessage = "A senha precisa ter no mínimo 7 digitos.")]
        [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d)(?=.*[^A-Za-z0-9]).+$", ErrorMessage = "A senha deve conter letra, número e caractere especial")]
        public string Senha { get; set; }
        public TipoUsuario? TipoUsuario { get; set; }
        public IFormFile? FotoPerfil { get; set; }
    }

    public class UsuarioGet
    {
        public int UsuarioId { get; set; }
        public string Nome { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public string Senha { get; set; }
        public string TipoUsuario { get; set; }
        public string? FotoPerfil { get; set; }
    }

    public class UsuarioLogin
    {
        public string LoginOuEmail { get; set; }
        public string Senha { get; set; }
    }

    public class UsuarioLoginResponse
    {
        public int UsuarioId { get; set; }
        public string Nome { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public string Senha { get; set; }
        public string TipoUsuario { get; set; }
        public string? FotoPerfil { get; set; }
    }

    public class UsuarioLoginResponseSeguro
    {
        public int UsuarioId { get; set; }
        public string Nome { get; set; }
        public string Login { get; set; }
        public string Email { get; set; }
        public string TipoUsuario { get; set; }
        public string? FotoPerfil { get; set; }
    }

}