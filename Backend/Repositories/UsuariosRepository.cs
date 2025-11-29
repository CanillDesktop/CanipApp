using Backend.Context;
using Backend.Models.Usuarios;
using Backend.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Backend.Repositories;

public class UsuariosRepository : IUsuariosRepository<UsuariosModel>
{
    private readonly CanilAppDbContext _context;

    public UsuariosRepository(CanilAppDbContext context)
    {
        _context = context;
    }

    public async Task<UsuariosModel> CreateAsync(UsuariosModel obj)
    {
        _context.Usuarios.Add(obj);
        await _context.SaveChangesAsync();
        return obj;
    }

    public async Task<UsuariosModel?> UpdateAsync(UsuariosModel obj)
    {
        _context.Usuarios.Update(obj);
        await _context.SaveChangesAsync();
        return obj;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario != null)
        {
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }

    public async Task<IEnumerable<UsuariosModel>> GetAsync()
    {
        return await _context.Usuarios.ToListAsync();
    }

    public async Task<UsuariosModel?> GetByIdAsync(int id)
    {
        return await _context.Usuarios.FindAsync(id);
    }

    public async Task<UsuariosModel?> GetByEmailAsync(string email)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<UsuariosModel?> GetByRefreshTokenAsync(string refreshToken)
    {
        return await _context.Usuarios
            .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
    }
}