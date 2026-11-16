using Clases;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business
{
    public class ReporteBusiness
    {
        /// <summary>Genera el contenido CSV de una lista de establecimientos.</summary>
        public string GenerarCsvEstablecimientos(IEnumerable<Establecimiento> establecimientos, string separador)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Join(separador, new[] { "ID", "Nombre", "Tipo", "Provincia", "Dirección", "Teléfono" }));

            foreach (Establecimiento e in establecimientos)
            {
                sb.AppendLine(string.Join(separador, new[]
                {
                    e.IdEstablecimiento.ToString(),
                    Escapar(e.Nombre, separador),
                    Escapar(e.NombreTipoEstablecimiento, separador),
                    Escapar(e.NombreProvincia, separador),
                    Escapar(e.Direccion, separador),
                    Escapar(e.Telefono, separador)
                }));
            }

            return sb.ToString();
        }

        private static string Escapar(string? valor, string separador)
        {
            valor = valor ?? "";
            bool requiereComillas = valor.Contains(separador) || valor.Contains('"') || valor.Contains('\n') || valor.Contains('\r');
            if (!requiereComillas)
                return valor;

            return "\"" + valor.Replace("\"", "\"\"") + "\"";
        }
    }
}
