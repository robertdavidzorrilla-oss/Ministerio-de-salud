using Clases;
using Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Business
{
    public class EstablecimientoBussines
    {
        private EstablecimientoData _datos = new EstablecimientoData();
        private ProvinciaData _provincias = new ProvinciaData();
        private TipoEstablecimientoData _tiposEstablecimiento = new TipoEstablecimientoData();

        public List<Establecimiento> ListarEstablecimientosSegunRol(Usuario usuarioActual)
        {
            List<Establecimiento> todos = _datos.Listar();

            // Si es Administrador general, ve todo
            if (usuarioActual.Rol == Roles.AdministradorGeneral)
            {
                return todos;
            }

            // Administrador provincial y Usuario consultor: solo su provincia
            if (usuarioActual.IdProvincia.HasValue)
            {
                return todos.Where(e => e.IdProvincia == usuarioActual.IdProvincia.Value).ToList();
            }

            return new List<Establecimiento>(); // Sin provincia asignada no ve nada
        }

        // Las provincias salen siempre de la tabla Provincias de la base de datos
        public List<Provinciacs> ObtenerProvincias()
        {
            return _provincias.Listar();
        }

        public List<TipoEstablecimiento> ObtenerTiposEstablecimiento()
        {
            return _tiposEstablecimiento.Listar();
        }

        public bool Guardar(Establecimiento obj, Usuario usuarioActual)
        {
            // Regla de negocio: Los consultores no pueden modificar nada
            if (usuarioActual.Rol == Roles.UsuarioConsultor)
            {
                throw new UnauthorizedAccessException("Los usuarios consultores no tienen permisos para registrar establecimientos.");
            }

            // El administrador provincial solo trabaja en su provincia
            if (usuarioActual.Rol == Roles.AdministradorProvincial)
            {
                if (!usuarioActual.IdProvincia.HasValue)
                    throw new UnauthorizedAccessException("Su usuario no tiene una provincia asignada.");

                obj.IdProvincia = usuarioActual.IdProvincia.Value;
            }

            // Validaciones básicas (los límites coinciden con las columnas de la tabla)
            obj.Nombre = (obj.Nombre ?? "").Trim();
            obj.Direccion = (obj.Direccion ?? "").Trim();
            obj.Telefono = (obj.Telefono ?? "").Trim();

            if (obj.Nombre.Length == 0)
                throw new ArgumentException("El nombre del establecimiento es requerido.");
            if (obj.Nombre.Length > 50)
                throw new ArgumentException("El nombre no puede pasar de 50 caracteres.");

            if (obj.Direccion.Length == 0)
                throw new ArgumentException("La dirección es requerida.");
            if (obj.Direccion.Length > 255)
                throw new ArgumentException("La dirección no puede pasar de 255 caracteres.");

            if (obj.Telefono.Length == 0)
                throw new ArgumentException("El teléfono es requerido.");
            if (obj.Telefono.Length > 12)
                throw new ArgumentException("El teléfono no puede pasar de 12 caracteres (ejemplo: 809-555-1234).");

            if (obj.IdProvincia <= 0)
                throw new ArgumentException("Debe seleccionar una provincia.");

            if (obj.IdTipoEstablecimiento <= 0)
                throw new ArgumentException("Debe seleccionar un tipo de establecimiento.");

            if (obj.IdEstablecimiento == 0)
            {
                return _datos.Insertar(obj);
            }
            else
            {
                VerificarAcceso(obj.IdEstablecimiento, usuarioActual);
                return _datos.Actualizar(obj);
            }
        }

        public bool Eliminar(int idEstablecimiento, Usuario usuarioActual)
        {
            if (usuarioActual.Rol == Roles.UsuarioConsultor)
            {
                throw new UnauthorizedAccessException("Los usuarios consultores no tienen permisos para eliminar registros.");
            }

            VerificarAcceso(idEstablecimiento, usuarioActual);
            return _datos.Eliminar(idEstablecimiento);
        }

        // Un administrador provincial no puede tocar establecimientos de otra provincia
        private void VerificarAcceso(int idEstablecimiento, Usuario usuarioActual)
        {
            bool visible = ListarEstablecimientosSegunRol(usuarioActual)
                .Any(e => e.IdEstablecimiento == idEstablecimiento);

            if (!visible)
                throw new UnauthorizedAccessException("No tiene permisos sobre un establecimiento de otra provincia.");
        }
    }
}
