using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clases
{
    public class Establecimiento
    {
        public int IdEstablecimiento { get; set; }
        public int IdTipoEstablecimiento { get; set; }
        public int IdProvincia { get; set; }
        public string Nombre { get; set; }
        public string Direccion { get; set; }
        public string Telefono { get; set; }

        
        public string NombreProvincia { get; set; }
        public string NombreTipoEstablecimiento { get; set; }

    }
}
