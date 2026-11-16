using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clases;

namespace Business
{
    internal class UsuarioBuilder
    {
        private Usuario _usuario;

        public UsuarioBuilder()
        {
            Reiniciar();
        }

        public void Reiniciar()
        {
            _usuario = new Usuario();
        }

        public UsuarioBuilder EstablecerId(int id)
        {
            _usuario.IdUsuario = id;
            return this;
        }

        public UsuarioBuilder EstablecerNombre(string nombre)
        {
            _usuario.Nombre = nombre;
            return this;
        }

        public UsuarioBuilder EstablecerCorreo(string correo)
        {
            _usuario.Correo = correo;
            return this;
        }

        public UsuarioBuilder EstablecerContraseñaPlana(string contraseñaPlana)
        {
            // Aplica el hash automáticamente al construir la contraseña
            _usuario.Contraseña = PasswordHelper.HashPassword(contraseñaPlana);
            return this;
        }

        public UsuarioBuilder EstablecerRolYProvincia(string rol, int? idProvincia)
        {
            _usuario.Rol = rol;
            if (rol == "Administrador general")
            {
                _usuario.IdProvincia = null; // El admin general no pertenece a una provincia específica
            }
            else
            {
                _usuario.IdProvincia = idProvincia;
            }
            return this;
        }

        public Usuario ObtenerUsuario()
        {
            Usuario resultado = _usuario;
            Reiniciar(); // Prepara el builder para otra instancia
            return resultado;
        }

    }
}
