using DataAccess.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalServices.ServicesResponses
{
    /// <summary>
    /// Representa el objeto de pagos que devuelve macro
    /// </summary>
    public class PaymentMacroResponse
    {
        public string TransaccionId { get; set; }
        public string Estado { get; set; }
        public DateTime Fecha { get; set; }
        public string Moneda { get; set; }
        public string Monto { get; set; }
        public string Productos { get; set; }
        public string MetodoPago { get; set; }
        public string Sucursal { get; set; }
        public string MedioPagoId { get; set; }
        public string MedioPagoNombre { get; set; }
        public string MedioPago { get; set; }
        public string TransaccionComercioId { get; set; }
        public int? EstadoId { get; set; }
        public int? Cuotas { get; set; }
        public int? UserId { get; set; }
        public int? BancoId { get; set; }
        public string TipoPago { get; set; }
        public string Informacion { get; set; }
        public decimal? MontoBruto { get; set; }
        public decimal? MontoDescuento { get; set; }
        public ResultMacroResponse Result { get; set; }
    }
}
