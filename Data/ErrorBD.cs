using Microsoft.Data.SqlClient;
using System;

namespace Data
{
    /// <summary>
    /// Convierte errores de SQL Server en mensajes entendibles para el usuario.
    /// </summary>
    public static class ErrorBD
    {
        public static string Traducir(Exception ex, string contexto)
        {
            Exception actual = ex;
            while (actual is not SqlException && actual.InnerException != null)
            {
                actual = actual.InnerException;
            }

            if (actual is SqlException sql)
            {
                switch (sql.Number)
                {
                    case 2627:
                    case 2601:
                        return contexto + ": ya existe un registro con ese valor único (por ejemplo, el correo o el teléfono ya están registrados).";
                    case 547:
                        return contexto + ": la operación choca con una relación entre datos (el registro está en uso por otros datos o la referencia no existe).";
                    case 2628:
                    case 8152:
                        return contexto + ": uno de los textos es demasiado largo para el campo.";
                }
            }

            return contexto + ": " + ex.Message;
        }
    }
}
