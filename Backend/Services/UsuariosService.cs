using Backend.Models.Usuarios;
using Backend.Repositories.Interfaces;
using Backend.Services.Interfaces;
using Shared.DTOs;

namespace Backend.Services
{
    public class UsuariosService : IUsuariosService
    {
        private readonly IUsuariosRepository<UsuariosModel> _repository;

        public UsuariosService(IUsuariosRepository<UsuariosModel> repository)
        {
            _repository = repository;
        }
        public async Task<UsuarioRequestDTO?> ValidarUsuarioAsync(string email, string senha)
        {
            var usuario = await _repository.GetByEmailAsync(email);
            if (usuario == null) return null;

            string? hash = senha.HashPassword256();

            var entradaValida = (usuario.Email == email && usuario.HashSenha == hash);
            return entradaValida ? usuario : null;
        }

        public async Task<UsuarioResponseDTO?> CriarAsync(UsuarioRequestDTO dto)
        {
            dto.Senha = dto.Senha!.HashPassword256();
            return await _repository.CreateAsync(dto);
        }

        public async Task<UsuarioResponseDTO?> BuscarPorIdAsync(int id) => await _repository.GetByIdAsync(id);

        public async Task<IEnumerable<UsuarioResponseDTO?>> BuscarTodosAsync() => (await _repository.GetAsync()).Select(u => (UsuarioResponseDTO?)u);

        public async Task<UsuarioResponseDTO?> AtualizarAsync(UsuarioRequestDTO dto) => await _repository.UpdateAsync(dto);

        public async Task<bool> DeletarAsync(int id) => await _repository.DeleteAsync(id);

        public async Task SalvarRefreshTokenAsync(int id, string refreshToken, DateTime expira) => await _repository.SaveRefreshTokenAsync(id, refreshToken, expira);

        public async Task<UsuarioResponseDTO?> BuscaPorRefreshTokenAsync(string? refreshToken)
        {
            if (refreshToken == null) return null;

            return await _repository.GetRefreshTokenAsync(refreshToken);
        }
    }
}
