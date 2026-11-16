using Clases;
using Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;

namespace Business
{
    public class UsuarioBusiness
    {
        private const int LargoMinimoPassword = 6;

        private readonly UsuarioData _datos = new UsuarioData();
        private readonly ProvinciaData _provincias = new ProvinciaData();

        // ===== LOGIN =====

        /// <summary>Devuelve el usuario si correo/nombre y contraseña son correctos; si no, null.</summary>
        public Usuario? Autenticar(string identificador, string passwordPlana)
        {
            if (string.IsNullOrWhiteSpace(identificador) || string.IsNullOrEmpty(passwordPlana))
                return null;

            return _datos.Autenticar(identificador.Trim(), PasswordHelper.HashPassword(passwordPlana));
        }

        // ===== CATÁLOGOS =====

        public List<Provinciacs> ObtenerProvincias()
        {
            return _provincias.Listar();
        }

        // ===== PERMISOS =====

        public bool PuedeGestionarUsuarios(Usuario actual)
        {
            return actual.Rol == Roles.AdministradorGeneral || actual.Rol == Roles.AdministradorProvincial;
        }

        /// <summary>Roles que el usuario actual puede asignar al crear/editar.</summary>
        public string[] RolesAsignables(Usuario actual)
        {
            if (actual.Rol == Roles.AdministradorGeneral)
                return Roles.Todos;
            if (actual.Rol == Roles.AdministradorProvincial)
                return new[] { Roles.UsuarioConsultor };
            return new string[0];
        }

        /// <summary>
        /// General: gestiona a todos. Provincial: solo consultores de su propia provincia.
        /// </summary>
        public bool PuedeGestionar(Usuario actual, Usuario objetivo)
        {
            if (actual.Rol == Roles.AdministradorGeneral)
                return true;

            if (actual.Rol == Roles.AdministradorProvincial)
                return actual.IdProvincia.HasValue
                    && objetivo.IdProvincia == actual.IdProvincia
                    && objetivo.Rol == Roles.UsuarioConsultor;

            return false;
        }

        private void ExigirGestion(Usuario actual)
        {
            if (!PuedeGestionarUsuarios(actual))
                throw new UnauthorizedAccessException("No tiene permisos para gestionar usuarios.");
        }

        // ===== CRUD =====

        public List<Usuario> ListarSegunRol(Usuario actual)
        {
            ExigirGestion(actual);
            List<Usuario> todos = _datos.Listar();

            if (actual.Rol == Roles.AdministradorGeneral)
                return todos;

            // Provincial: ve a los usuarios de su provincia
            return todos.Where(u => actual.IdProvincia.HasValue && u.IdProvincia == actual.IdProvincia).ToList();
        }

        /// <summary>
        /// Crea (IdUsuario == 0) o actualiza un usuario. En la creación la contraseña es obligatoria;
        /// en la edición se ignora (use RestablecerContraseña).
        /// </summary>
        public bool Guardar(Usuario obj, string? passwordPlana, Usuario actual)
        {
            ExigirGestion(actual);

            string nombre = (obj.Nombre ?? "").Trim();
            string correo = (obj.Correo ?? "").Trim();

            if (nombre.Length == 0)
                throw new ArgumentException("El nombre es requerido.");
            if (nombre.Length > 50)
                throw new ArgumentException("El nombre no puede pasar de 50 caracteres.");

            if (correo.Length == 0)
                throw new ArgumentException("El correo es requerido.");
            if (correo.Length > 100 || !CorreoValido(correo))
                throw new ArgumentException("El correo no tiene un formato válido.");

            if (!Roles.Todos.Contains(obj.Rol))
                throw new ArgumentException("Debe seleccionar un rol válido.");

            if (!RolesAsignables(actual).Contains(obj.Rol))
                throw new UnauthorizedAccessException("No tiene permisos para asignar el rol '" + obj.Rol + "'.");

            // Un provincial solo puede crear/editar dentro de su provincia
            int? idProvincia = obj.IdProvincia;
            if (actual.Rol == Roles.AdministradorProvincial)
            {
                if (!actual.IdProvincia.HasValue)
                    throw new UnauthorizedAccessException("Su usuario no tiene una provincia asignada.");
                idProvincia = actual.IdProvincia;
            }

            if (obj.Rol != Roles.AdministradorGeneral && (!idProvincia.HasValue || idProvincia.Value <= 0))
                throw new ArgumentException("Debe seleccionar una provincia.");

            if (obj.IdUsuario == 0)
            {
                if (string.IsNullOrEmpty(passwordPlana) || passwordPlana.Length < LargoMinimoPassword)
                    throw new ArgumentException("La contraseña debe tener al menos " + LargoMinimoPassword + " caracteres.");

                Usuario nuevo = new UsuarioBuilder()
                    .EstablecerNombre(nombre)
                    .EstablecerCorreo(correo)
                    .EstablecerContraseñaPlana(passwordPlana)
                    .EstablecerRolYProvincia(obj.Rol, idProvincia)
                    .ObtenerUsuario();

                return _datos.Insertar(nuevo);
            }

            Usuario? existente = _datos.Listar().FirstOrDefault(u => u.IdUsuario == obj.IdUsuario);
            if (existente == null)
                throw new ArgumentException("El usuario ya no existe.");

            if (!PuedeGestionar(actual, existente))
                throw new UnauthorizedAccessException("No tiene permisos para modificar este usuario.");

            if (existente.IdUsuario == actual.IdUsuario && existente.Rol != obj.Rol)
                throw new InvalidOperationException("No puede cambiar su propio rol.");

            Usuario editado = new UsuarioBuilder()
                .EstablecerId(obj.IdUsuario)
                .EstablecerNombre(nombre)
                .EstablecerCorreo(correo)
                .EstablecerRolYProvincia(obj.Rol, idProvincia)
                .ObtenerUsuario();

            return _datos.Actualizar(editado);
        }

        public bool Eliminar(Usuario objetivo, Usuario actual)
        {
            ExigirGestion(actual);

            if (objetivo.IdUsuario == actual.IdUsuario)
                throw new InvalidOperationException("No puede eliminar su propio usuario.");

            if (!PuedeGestionar(actual, objetivo))
                throw new UnauthorizedAccessException("No tiene permisos para eliminar este usuario.");

            return _datos.Eliminar(objetivo.IdUsuario);
        }

        // ===== CONTRASEÑAS =====

        /// <summary>Un administrador define una contraseña nueva para otro usuario.</summary>
        public bool RestablecerContraseña(Usuario objetivo, string nuevaPlana, Usuario actual)
        {
            ExigirGestion(actual);

            if (!PuedeGestionar(actual, objetivo))
                throw new UnauthorizedAccessException("No tiene permisos para cambiar la contraseña de este usuario.");

            ValidarPasswordNueva(nuevaPlana);
            return _datos.CambiarContraseña(objetivo.IdUsuario, PasswordHelper.HashPassword(nuevaPlana));
        }

        /// <summary>El usuario cambia su propia contraseña confirmando la actual.</summary>
        public bool CambiarContraseña(Usuario actual, string passwordActual, string nuevaPlana, string confirmacion)
        {
            if (string.IsNullOrEmpty(passwordActual))
                throw new ArgumentException("Ingrese su contraseña actual.");

            ValidarPasswordNueva(nuevaPlana);

            if (nuevaPlana != confirmacion)
                throw new ArgumentException("La confirmación no coincide con la nueva contraseña.");

            if (nuevaPlana == passwordActual)
                throw new ArgumentException("La nueva contraseña debe ser diferente a la actual.");

            Usuario? verificado = _datos.Autenticar(actual.Correo, PasswordHelper.HashPassword(passwordActual));
            if (verificado == null || verificado.IdUsuario != actual.IdUsuario)
                throw new ArgumentException("La contraseña actual es incorrecta.");

            return _datos.CambiarContraseña(actual.IdUsuario, PasswordHelper.HashPassword(nuevaPlana));
        }

        private static void ValidarPasswordNueva(string? nuevaPlana)
        {
            if (string.IsNullOrEmpty(nuevaPlana) || nuevaPlana.Length < LargoMinimoPassword)
                throw new ArgumentException("La contraseña debe tener al menos " + LargoMinimoPassword + " caracteres.");
        }

        private static bool CorreoValido(string correo)
        {
            try
            {
                MailAddress direccion = new MailAddress(correo);
                return direccion.Address == correo;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}
