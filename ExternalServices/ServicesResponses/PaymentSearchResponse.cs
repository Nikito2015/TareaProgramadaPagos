using DataAccess.Repositories;
using MercadoPago.DataStructures.Payment;
using MercadoPago.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalServices.ServicesResponses
{
    /// <summary>
    /// Representa el objeto de pagos que devuelve mercado pago
    /// </summary>
    public class PaymentSearchResponse 
    {
      public List<PaymentItem> results { get; set; } = new List<PaymentItem>();
    }
  public class PaymentItem
    {
        public string Status { get; set; }
        public long Id { get; set; }
        public MerchantOrderItem Order { get; set; }

    }
 
}
