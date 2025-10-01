using Backend.Models.Usuarios;

namespace Backend.Services
{
    public class UsuariosService
    {
        private static readonly List<UsuariosModel> _usuarios = [];

        public UsuariosService() { }

        public UsuariosModel? ValidaUsuario(string email, string senha)
        {
            var usuario = _usuarios.FirstOrDefault(u =>  u.Email == email);
            if (usuario == null) return null;

            string? hash = senha.HashPassword256();

            var entradaValida = usuario.Email == email && usuario.HashSenha == hash;
            return entradaValida ? usuario : null;
        }

        public void CriaUsuario(UsuariosModel? model)
        {
            ArgumentNullException.ThrowIfNull(model);
            model.HashSenha = model.HashSenha?.HashPassword256();

            _usuarios.Add(model);
        }

        public UsuariosModel? BuscaPorId(int id) => _usuarios.FirstOrDefault(u => u.Id == id);

        public IEnumerable<UsuariosModel>? BuscarTodos() => _usuarios;

        public void Atualizar(UsuariosModel model, int id)
        {
            var usuario = _usuarios.Find(u => u.Id == id) ?? throw new ArgumentNullException(nameof(model));

            usuario = model;
        }

        public void Deletar(int id)
        {
            var usuario = _usuarios.Find(u => u.Id == id) ?? throw new ArgumentNullException();

            _usuarios.Remove(usuario);
        }

        internal void SalvarRefreshToken(int id, string refreshToken, DateTime expira)
        {
            var usuario = _usuarios.FirstOrDefault(u => u.Id == id);
            if (usuario != null)
            {
                usuario.RefreshToken = refreshToken;
                usuario.DataHoraExpiracaoRefreshToken = expira;
            }
        }

        internal UsuariosModel? BuscaPorRefreshToken(string? refreshToken)
        {
            if (refreshToken == null) return null;

            var usuario = _usuarios.FirstOrDefault(u =>
                u.RefreshToken == refreshToken &&
                u.DataHoraExpiracaoRefreshToken > DateTime.Now);

            return usuario ?? null;
        }
    }
}
