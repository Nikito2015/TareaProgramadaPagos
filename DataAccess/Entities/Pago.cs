using DataAccess.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Entities
{
    public class Pago
    {
        public int IdPago { get; set; }

        public int Estado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string NumeroFactura { get; set; }
        public decimal Importe { get; set; }
        public string Preference { get; set; }
        public string Collection { get; set; }
        public string MerchantOrder { get; set; }
        public bool? Procesado { get; set; }
        public int? IdSocio { get; set; }
        public int? IdConexion { get; set; }

    }
}
