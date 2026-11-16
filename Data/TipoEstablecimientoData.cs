using Clases;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public class TipoEstablecimientoData
    {
        public List<TipoEstablecimiento> Listar()
        {
            List<TipoEstablecimiento> lista = new List<TipoEstablecimiento>();
            SqlConnection conexion = null;

            try
            {
                conexion = ConexionBD.ObtenerInstancia().CrearConexion();
                string query = "SELECT idTipoEstablecimiento, nombre, descripcion FROM tiposEstablecimientos";

                using (SqlCommand cmd = new SqlCommand(query, conexion))
                {
                    cmd.CommandType = CommandType.Text;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new TipoEstablecimiento()
                            {
                                IdTipoEstablecimiento = Convert.ToInt32(reader["idTipoEstablecimiento"]),
                                Nombre = reader["nombre"].ToString(),
                                Descripcion = reader["descripcion"].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ErrorBD.Traducir(ex, "Error al listar tipos de establecimientos"), ex);
            }
            finally
            {
                if (conexion != null && conexion.State == ConnectionState.Open)
                    conexion.Close();
            }

            return lista;
        }

        public bool Insertar(TipoEstablecimiento obj)
        {
            bool respuesta = false;
            SqlConnection conexion = null;

            try
            {
                conexion = ConexionBD.ObtenerInstancia().CrearConexion();
                using (SqlCommand cmd = new SqlCommand("sp_InsertarTipoEstablecimiento", conexion))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@nombre", obj.Nombre);
                    cmd.Parameters.AddWithValue("@descripcion", obj.Descripcion);

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    if (filasAfectadas > 0) respuesta = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ErrorBD.Traducir(ex, "Error al insertar tipo de establecimiento"), ex);
            }
            finally
            {
                if (conexion != null && conexion.State == ConnectionState.Open)
                    conexion.Close();
            }

            return respuesta;
        }

        public bool Actualizar(TipoEstablecimiento obj)
        {
            bool respuesta = false;
            SqlConnection conexion = null;

            try
            {
                conexion = ConexionBD.ObtenerInstancia().CrearConexion();
                using (SqlCommand cmd = new SqlCommand("sp_ActualizarTipoEstablecimiento", conexion))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@idTipoEstablecimiento", obj.IdTipoEstablecimiento);
                    cmd.Parameters.AddWithValue("@nombre", obj.Nombre);
                    cmd.Parameters.AddWithValue("@descripcion", obj.Descripcion);

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    if (filasAfectadas > 0) respuesta = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ErrorBD.Traducir(ex, "Error al actualizar tipo de establecimiento"), ex);
            }
            finally
            {
                if (conexion != null && conexion.State == ConnectionState.Open)
                    conexion.Close();
            }

            return respuesta;
        }

        public bool Eliminar(int idTipoEstablecimiento)
        {
            bool respuesta = false;
            SqlConnection conexion = null;

            try
            {
                conexion = ConexionBD.ObtenerInstancia().CrearConexion();
                using (SqlCommand cmd = new SqlCommand("sp_EliminarTipoEstablecimiento", conexion))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@idTipoEstablecimiento", idTipoEstablecimiento);

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    if (filasAfectadas > 0) respuesta = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ErrorBD.Traducir(ex, "Error al eliminar tipo de establecimiento"), ex);
            }
            finally
            {
                if (conexion != null && conexion.State == ConnectionState.Open)
                    conexion.Close();
            }

            return respuesta;
        }

    }
}
