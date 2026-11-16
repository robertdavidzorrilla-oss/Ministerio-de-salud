using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Clases;
namespace Data
{
    public class EstablecimientoData
    {
        public List<Establecimiento> Listar()
        {
            List<Establecimiento> lista = new List<Establecimiento>();
            SqlConnection conexion = null;

            try
            {
                conexion = ConexionBD.ObtenerInstancia().CrearConexion();
                string query = "SELECT idEstablecimiento, idTipoEstablecimiento, idProvincia, NombreEstablecimiento, direccion, telefono, NombreProvincia, NombreTipoEstablecimiento FROM vw_ListarEstablecimientos";

                using (SqlCommand cmd = new SqlCommand(query, conexion))
                {
                    cmd.CommandType = CommandType.Text;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Establecimiento()
                            {
                                IdEstablecimiento = Convert.ToInt32(reader["idEstablecimiento"]),
                                IdTipoEstablecimiento = Convert.ToInt32(reader["idTipoEstablecimiento"]),
                                IdProvincia = Convert.ToInt32(reader["idProvincia"]),
                                Nombre = reader["NombreEstablecimiento"].ToString(),
                                Direccion = reader["direccion"].ToString(),
                                Telefono = reader["telefono"].ToString(),
                                NombreProvincia = reader["NombreProvincia"].ToString(),
                                NombreTipoEstablecimiento = reader["NombreTipoEstablecimiento"].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ErrorBD.Traducir(ex, "Error al listar establecimientos"), ex);
            }
            finally
            {
                if (conexion != null && conexion.State == ConnectionState.Open)
                    conexion.Close();
            }

            return lista;
        }

        
        public bool Insertar(Establecimiento obj)
        {
            bool respuesta = false;
            SqlConnection conexion = null;

            try
            {
                conexion = ConexionBD.ObtenerInstancia().CrearConexion();
                using (SqlCommand cmd = new SqlCommand("sp_InsertarEstablecimiento", conexion))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@idTipoEstablecimiento", obj.IdTipoEstablecimiento);
                    cmd.Parameters.AddWithValue("@idProvincia", obj.IdProvincia);
                    cmd.Parameters.AddWithValue("@nombre", obj.Nombre);
                    cmd.Parameters.AddWithValue("@direccion", obj.Direccion);
                    cmd.Parameters.AddWithValue("@telefono", obj.Telefono);

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    if (filasAfectadas > 0) respuesta = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ErrorBD.Traducir(ex, "Error al insertar establecimiento"), ex);
            }
            finally
            {
                if (conexion != null && conexion.State == ConnectionState.Open)
                    conexion.Close();
            }

            return respuesta;
        }

     
        public bool Actualizar(Establecimiento obj)
        {
            bool respuesta = false;
            SqlConnection conexion = null;

            try
            {
                conexion = ConexionBD.ObtenerInstancia().CrearConexion();
                using (SqlCommand cmd = new SqlCommand("sp_ActualizarEstablecimiento", conexion))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@idEstablecimiento", obj.IdEstablecimiento);
                    cmd.Parameters.AddWithValue("@idTipoEstablecimiento", obj.IdTipoEstablecimiento);
                    cmd.Parameters.AddWithValue("@idProvincia", obj.IdProvincia);
                    cmd.Parameters.AddWithValue("@nombre", obj.Nombre);
                    cmd.Parameters.AddWithValue("@direccion", obj.Direccion);
                    cmd.Parameters.AddWithValue("@telefono", obj.Telefono);

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    if (filasAfectadas > 0) respuesta = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ErrorBD.Traducir(ex, "Error al actualizar establecimiento"), ex);
            }
            finally
            {
                if (conexion != null && conexion.State == ConnectionState.Open)
                    conexion.Close();
            }

            return respuesta;
        }

      
        public bool Eliminar(int idEstablecimiento)
        {
            bool respuesta = false;
            SqlConnection conexion = null;

            try
            {
                conexion = ConexionBD.ObtenerInstancia().CrearConexion();
                using (SqlCommand cmd = new SqlCommand("sp_EliminarEstablecimiento", conexion))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@idEstablecimiento", idEstablecimiento);

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    if (filasAfectadas > 0) respuesta = true;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ErrorBD.Traducir(ex, "Error al eliminar establecimiento"), ex);
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
