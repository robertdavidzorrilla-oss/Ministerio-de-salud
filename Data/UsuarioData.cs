using Clases;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace Data
{
    public class UsuarioData
    {
        private const string ColumnasVista = "idUsuario, idProvincia, nombre, correo, contraseña, rol, NombreProvincia";

        private static Usuario Mapear(SqlDataReader reader, bool incluirHash)
        {
            return new Usuario()
            {
                IdUsuario = Convert.ToInt32(reader["idUsuario"]),
                IdProvincia = reader["idProvincia"] != DBNull.Value ? Convert.ToInt32(reader["idProvincia"]) : (int?)null,
                Nombre = reader["nombre"].ToString() ?? "",
                Correo = reader["correo"].ToString() ?? "",
                Contraseña = incluirHash ? (reader["contraseña"].ToString() ?? "") : "",
                Rol = reader["rol"].ToString() ?? "",
                NombreProvincia = reader["NombreProvincia"].ToString() ?? ""
            };
        }

        /// <summary>Lista todos los usuarios (sin devolver el hash de contraseña).</summary>
        public List<Usuario> Listar()
        {
            List<Usuario> lista = new List<Usuario>();

            try
            {
                using (SqlConnection conexion = ConexionBD.ObtenerInstancia().CrearConexion())
                using (SqlCommand cmd = new SqlCommand("SELECT " + ColumnasVista + " FROM vw_ListarUsuarios ORDER BY nombre", conexion))
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(Mapear(reader, false));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ErrorBD.Traducir(ex, "Error al listar usuarios"), ex);
            }

            return lista;
        }

        /// <summary>
        /// Busca un usuario por correo o nombre cuyo hash de contraseña coincida.
        /// Devuelve null si las credenciales no son válidas.
        /// </summary>
        public Usuario? Autenticar(string identificador, string passwordHash)
        {
            try
            {
                using (SqlConnection conexion = ConexionBD.ObtenerInstancia().CrearConexion())
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT " + ColumnasVista + " FROM vw_ListarUsuarios " +
                    "WHERE (correo = @identificador OR nombre = @identificador) AND contraseña = @hash", conexion))
                {
                    cmd.Parameters.Add("@identificador", SqlDbType.VarChar, 100).Value = identificador;
                    cmd.Parameters.Add("@hash", SqlDbType.NVarChar, 255).Value = passwordHash;

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return Mapear(reader, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ErrorBD.Traducir(ex, "Error al validar el usuario"), ex);
            }

            return null;
        }

        public bool Insertar(Usuario obj)
        {
            try
            {
                return ConexionBD.ObtenerInstancia().EjecutarProcedimiento("sp_InsertarUsuario",
                    ConexionBD.Parametro("@idProvincia", obj.IdProvincia),
                    ConexionBD.Parametro("@nombre", obj.Nombre),
                    ConexionBD.Parametro("@correo", obj.Correo),
                    ConexionBD.Parametro("@contraseña", obj.Contraseña),
                    ConexionBD.Parametro("@rol", obj.Rol));
            }
            catch (Exception ex)
            {
                throw new Exception(ErrorBD.Traducir(ex, "Error al insertar usuario"), ex);
            }
        }

        public bool Actualizar(Usuario obj)
        {
            try
            {
                return ConexionBD.ObtenerInstancia().EjecutarProcedimiento("sp_ActualizarUsuario",
                    ConexionBD.Parametro("@idUsuario", obj.IdUsuario),
                    ConexionBD.Parametro("@idProvincia", obj.IdProvincia),
                    ConexionBD.Parametro("@nombre", obj.Nombre),
                    ConexionBD.Parametro("@correo", obj.Correo),
                    ConexionBD.Parametro("@rol", obj.Rol));
            }
            catch (Exception ex)
            {
                throw new Exception(ErrorBD.Traducir(ex, "Error al actualizar usuario"), ex);
            }
        }

        public bool CambiarContraseña(int idUsuario, string nuevoHash)
        {
            try
            {
                return ConexionBD.ObtenerInstancia().EjecutarProcedimiento("sp_CambiarContraseñaUsuario",
                    ConexionBD.Parametro("@idUsuario", idUsuario),
                    ConexionBD.Parametro("@nuevaContraseña", nuevoHash));
            }
            catch (Exception ex)
            {
                throw new Exception(ErrorBD.Traducir(ex, "Error al cambiar la contraseña"), ex);
            }
        }

        public bool Eliminar(int idUsuario)
        {
            try
            {
                return ConexionBD.ObtenerInstancia().EjecutarProcedimiento("sp_EliminarUsuario",
                    ConexionBD.Parametro("@idUsuario", idUsuario));
            }
            catch (Exception ex)
            {
                throw new Exception(ErrorBD.Traducir(ex, "Error al eliminar usuario"), ex);
            }
        }
    }
}
