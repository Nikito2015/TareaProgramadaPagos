using System.Collections.Generic;

namespace ExternalServices.ServicesResponses
{
    /// <summary>
    /// Representa el objeto que devuelve mercado pago
    /// </summary>

    public class MerchantOrderResponse
    {
        public List<MerchantOrderItem> Elements { get; set; } = new List<MerchantOrderItem>();       
    }
    public class MerchantOrderItem
    {
        public string Id { get; set; }
        public string Preference_Id { get; set; }
    }
}
