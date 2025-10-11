using Backend.Context;
using Backend.Models.Usuarios;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories
{
    public class UsuariosRepository : IUsuariosRepository<UsuariosModel>
    {
        private readonly CanilAppDbContext _context;

        public UsuariosRepository(CanilAppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UsuariosModel>> GetAsync()
        {
            var usuarios = await _context.Usuarios.ToListAsync();
            return usuarios;
        }

        public async Task<UsuariosModel?> GetByEmailAsync(string email)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Email == email);

            ArgumentNullException.ThrowIfNull(usuario);

            return usuario;
        }

        public async Task<UsuariosModel?> GetByIdAsync(int id)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);

            ArgumentNullException.ThrowIfNull(usuario);

            return usuario;
        }

        public async Task<UsuariosModel> CreateAsync(UsuariosModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            await _context.Usuarios.AddAsync(model);
            await _context.SaveChangesAsync();

            return model;
        }

        public async Task<UsuariosModel?> UpdateAsync(UsuariosModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            _context.Entry(model).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return model;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            ArgumentNullException.ThrowIfNull(usuario);

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task SaveRefreshTokenAsync(int id, string refreshToken, DateTime expira)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
            if (usuario != null)
            {
                usuario.RefreshToken = refreshToken;
                usuario.DataHoraExpiracaoRefreshToken = expira;
                _context.Entry(usuario).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
            ArgumentNullException.ThrowIfNull(usuario);
        }

        public async Task<UsuariosModel?> GetRefreshTokenAsync(string? refreshToken)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u =>
                u.RefreshToken == refreshToken &&
                u.DataHoraExpiracaoRefreshToken > DateTime.Now);

            return usuario ?? null;
        }
    }
}
