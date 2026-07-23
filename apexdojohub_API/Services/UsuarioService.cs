using apexdojohub_API.Helpers;
using apexdojohub_API.Interface;
using apexdojohub_API.Models.Usuario;
using Microsoft.Data.SqlClient;
using System.Data;

namespace apexdojohub_API.Services
{
    public class UsuarioService : IUsuarioInterface
    {
        private readonly IDbConnection _connection;

        public UsuarioService(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<List<UsuarioGet>> ConsultarUsuario(int usuarioId)
        {
            try
            {
                var usuarios = new List<UsuarioGet>();
                using var command = _connection.CreateStoredProcedure(Constantes.Constantes.CONSULTARUSUARIO);

                command.Parameters.Add(new SqlParameter("@UsuarioId", usuarioId));

                if (_connection.State != ConnectionState.Open)
                    await ((SqlConnection)_connection).OpenAsync();

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    usuarios.Add(new UsuarioGet
                    {
                        UsuarioId = Convert.ToInt32(reader["UsuarioId"]),
                        Nome = reader["Nome"].ToString(),
                        Login = reader["Login"].ToString(),
                        Email = reader["Email"].ToString(),
                        Senha = reader["Senha"].ToString(),
                        TipoUsuario = reader["TipoUsuario"].ToString(),
                        FotoPerfil = reader["FotoPerfil"] != DBNull.Value ? Convert.ToBase64String((byte[])reader["FotoPerfil"]) : null
                    });
                }
                return usuarios;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao consultar usuário: " + ex.Message);

            }
        }

        public async Task CriarUsuario(UsuarioPost usuario)
        {
            try
            {
                var senha = BCrypt.Net.BCrypt.HashPassword(usuario.Senha);
                var fotoBase64 = await Util.Util.ConverterParaBase64(usuario.FotoPerfil);
                using var command = _connection.CreateStoredProcedure(Constantes.Constantes.CRIARUSUARIO);

                command.Parameters.Add(new SqlParameter("@Nome", usuario.Nome));
                command.Parameters.Add(new SqlParameter("@Login", usuario.Login));
                command.Parameters.Add(new SqlParameter("@Email", usuario.Email));
                command.Parameters.Add(new SqlParameter("@Senha", senha));
                command.Parameters.Add(new SqlParameter("@TipoUsuario", usuario.TipoUsuario));
                command.Parameters.Add(new SqlParameter("@FotoPerfil", fotoBase64 ?? (object)DBNull.Value));

                if (_connection.State != ConnectionState.Open)
                    await ((SqlConnection)_connection).OpenAsync();

                await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao criar usuário: " + ex.Message);
            }
        }

        public async Task<UsuarioLoginResponse> LoginUsuario(string loginOuEmail)
        {
            try
            {
                var logins = new UsuarioLoginResponse();
                using var command = _connection.CreateStoredProcedure(Constantes.Constantes.LOGINUSUARIO);

                command.Parameters.Add(new SqlParameter("@LoginOuEmail", loginOuEmail));

                if (_connection.State != ConnectionState.Open)
                    await ((SqlConnection)_connection).OpenAsync();

                using var reader = command.ExecuteReader();

                while (await reader.ReadAsync())
                {
                    logins = new UsuarioLoginResponse
                    {
                        UsuarioId = Convert.ToInt32(reader["UsuarioId"]),
                        Nome = reader["Nome"].ToString(),
                        Login = reader["Login"].ToString(),
                        Email = reader["Email"].ToString(),
                        Senha = reader["Senha"].ToString(),
                        TipoUsuario = reader["TipoUsuario"].ToString(),
                        FotoPerfil = reader["FotoPerfil"] != DBNull.Value ? Convert.ToBase64String((byte[])reader["FotoPerfil"]) : null
                    };

                }
                return logins;
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao realizar login do usuário: " + ex.Message);
            }
        }
    }
}

