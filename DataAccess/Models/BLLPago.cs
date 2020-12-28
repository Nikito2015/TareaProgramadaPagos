using DataAccess.Entities;
using DataAccess.Repositories;

namespace DataAccess.Models
{
    public class BLLPago : Pago
    {
        public string NombreEstado => ((EstadosPagos)Estado).ToString();
    }

}
