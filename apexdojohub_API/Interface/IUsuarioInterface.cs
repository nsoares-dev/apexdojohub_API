using apexdojohub_API.Models.Usuario;

namespace apexdojohub_API.Interface
{
    public interface IUsuarioInterface
    {
        Task CriarUsuario(UsuarioPost usuario);
        Task<List<UsuarioGet>> ConsultarUsuario(int usuarioId);
        Task<UsuarioLoginResponse> LoginUsuario(string loginOuEmail);
    }
}
