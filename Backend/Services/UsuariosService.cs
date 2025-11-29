using Backend.Models.Usuarios;
using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;
using Shared.DTOs;

namespace Backend.Services;

public class UsuariosService : IUsuariosService<UsuarioResponseDTO>
{
    private readonly IUsuariosRepository<UsuariosModel> _repository;

    public UsuariosService(IUsuariosRepository<UsuariosModel> repository)
    {
        _repository = repository;
    }

    public async Task<UsuarioResponseDTO?> CriarAsync(UsuarioRequestDTO dto)
    {
        try
        {
            var usuarioModel = UsuariosModel.FromDTO(dto);
            usuarioModel.HashSenha = BCrypt.Net.BCrypt.HashPassword(dto.Senha);

            var usuarioCriado = await _repository.CreateAsync(usuarioModel);
            return usuarioCriado?.ToDTO();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao criar usuário", ex);
        }
    }

    public async Task<UsuarioResponseDTO?> AtualizarAsync(UsuarioRequestDTO dto)
    {
        try
        {
            var usuarioModel = UsuariosModel.FromDTO(dto);

            if (!string.IsNullOrEmpty(dto.Senha))
            {
                usuarioModel.HashSenha = BCrypt.Net.BCrypt.HashPassword(dto.Senha);
            }

            var atualizado = await _repository.UpdateAsync(usuarioModel);
            return atualizado?.ToDTO();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Erro ao atualizar usuário", ex);
        }
    }

    public async Task<bool> DeletarAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<IEnumerable<UsuarioResponseDTO>> BuscarTodosAsync()
    {
        var usuarios = await _repository.GetAsync();
        return usuarios.Select(u => u.ToDTO()).ToList();
    }

    public async Task<UsuarioResponseDTO?> BuscarPorIdAsync(int id)
    {
        var usuario = await _repository.GetByIdAsync(id);
        return usuario?.ToDTO();
    }

    public async Task<UsuarioResponseDTO?> ValidarUsuarioAsync(string login, string senha)
    {
        try
        {
            var usuario = await _repository.GetByEmailAsync(login);

            if (usuario == null)
                return null;

            var senhaValida = BCrypt.Net.BCrypt.Verify(senha, usuario.HashSenha);

            return senhaValida ? usuario.ToDTO() : null;
        }
        catch
        {
            return null;
        }
    }

    public async Task SalvarRefreshTokenAsync(int usuarioId, string refreshToken, DateTime expiry)
    {
        var usuario = await _repository.GetByIdAsync(usuarioId);

        if (usuario == null)
            throw new ArgumentNullException(nameof(usuario), "Usuário não encontrado");

        usuario.RefreshToken = refreshToken;
        usuario.DataHoraExpiracaoRefreshToken = expiry;

        await _repository.UpdateAsync(usuario);
    }

    public async Task<UsuarioResponseDTO?> BuscaPorRefreshTokenAsync(string? refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
            return null;

        var usuario = await _repository.GetByRefreshTokenAsync(refreshToken);

        if (usuario == null)
            return null;

        if (usuario.DataHoraExpiracaoRefreshToken < DateTime.UtcNow)
            return null;

        return usuario.ToDTO();
    }
}