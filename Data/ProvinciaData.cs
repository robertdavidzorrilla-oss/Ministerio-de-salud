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
    public class ProvinciaData
    {
        public List<Provinciacs> Listar()
        {
            List<Provinciacs> lista = new List<Provinciacs>();
            SqlConnection conexion = null;

            try
            {
                conexion = ConexionBD.ObtenerInstancia().CrearConexion();
                string query = "SELECT idProvincia, nombre FROM Provincias ORDER BY nombre";

                using (SqlCommand cmd = new SqlCommand(query, conexion))
                {
                    cmd.CommandType = CommandType.Text;
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Provinciacs()
                            {
                                IdProvincia = Convert.ToInt32(reader["idProvincia"]),
                                Nombre = reader["nombre"].ToString()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ErrorBD.Traducir(ex, "Error al listar provincias"), ex);
            }
            finally
            {
                if (conexion != null && conexion.State == ConnectionState.Open)
                    conexion.Close();
            }

            return lista;
        }
    }
}
