using Microsoft.Data.SqlClient;
using System;
using System.Data;
using System.IO;

namespace Data
{
    public class ConexionBD
    {
        // Cadena por defecto (la de tu equipo). Se puede sobreescribir sin recompilar:
        //   1) variable de entorno MINISTERIO_SALUD_CONNSTR, o
        //   2) un archivo "conexion.txt" junto al .exe con la cadena de conexión completa.
        private const string CadenaPorDefecto =
            "Server=DESKTOP-QPRRR2N;Database=MinisterioDeSalud;User Id=sa;Password=fcharles4;Encrypt=True;TrustServerCertificate=True;";

        private static readonly object _bloqueo = new object();
        private static ConexionBD? _instance = null;
        private readonly string _cadenaConexion;

        // Constructor privado para evitar la instanciación directa con 'new'
        private ConexionBD()
        {
            _cadenaConexion = ObtenerCadenaConexion();
        }

        private static string ObtenerCadenaConexion()
        {
            string? deEntorno = Environment.GetEnvironmentVariable("MINISTERIO_SALUD_CONNSTR");
            if (!string.IsNullOrWhiteSpace(deEntorno))
                return deEntorno;

            string archivo = Path.Combine(AppContext.BaseDirectory, "conexion.txt");
            if (File.Exists(archivo))
            {
                string texto = File.ReadAllText(archivo).Trim();
                if (!string.IsNullOrWhiteSpace(texto))
                    return texto;
            }

            return CadenaPorDefecto;
        }

        // Método público estático para obtener la única instancia (Singleton)
        public static ConexionBD ObtenerInstancia()
        {
            lock (_bloqueo)
            {
                if (_instance == null)
                {
                    _instance = new ConexionBD();
                }
                return _instance;
            }
        }

        // Método para abrir y retornar la conexión SQL
        public SqlConnection CrearConexion()
        {
            SqlConnection conexion = new SqlConnection(_cadenaConexion);
            try
            {
                conexion.Open();
            }
            catch (Exception ex)
            {
                conexion.Dispose();
                throw new Exception("No se pudo conectar con la base de datos: " + ex.Message, ex);
            }
            return conexion;
        }

        /// <summary>Crea un parámetro convirtiendo null en DBNull.</summary>
        public static SqlParameter Parametro(string nombre, object? valor)
        {
            return new SqlParameter(nombre, valor ?? DBNull.Value);
        }

        /// <summary>Ejecuta un procedimiento almacenado de escritura. True si afectó filas.</summary>
        public bool EjecutarProcedimiento(string procedimiento, params SqlParameter[] parametros)
        {
            using (SqlConnection conexion = CrearConexion())
            using (SqlCommand cmd = new SqlCommand(procedimiento, conexion))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddRange(parametros);
                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}
