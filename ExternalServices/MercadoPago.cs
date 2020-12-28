using System;
using System.Collections.Generic;
using System.Linq;
using MercadoPago;
using System.Configuration;
using log4net;
using Newtonsoft.Json;
using ExternalServices.ServicesResponses;
using ExternalServices.Common;
using System.Net.Http;
using System.Net;

namespace ExternalServices
{


    /// <summary>
    /// Contiene los métodos de consulta a Mercado Pago
    /// </summary>
    public class MercadoPago
    {

        #region Private Members
        public static string MP_APPROVED_STATUS = "approved";
        public static string MP_REJECTED_STATUS = "rejected";
        private bool _isSandbox;
        private string _mpApiEndpoint;
        private string _token;
        private ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor
        /// </summary>
        public MercadoPago()
        {
            log.Info("MercadoPago() Ctor.");
            try
            {
                _isSandbox = Convert.ToBoolean(ConfigurationManager.AppSettings["isSandbox"].ToString());
                log.Info("IsSandbox: " + _isSandbox);
                _mpApiEndpoint = ConfigurationManager.AppSettings["mpApiEndpoint"].ToString();
                log.Info("Endpoint MP: " + _mpApiEndpoint);
                ReEvaluarYSetearMercadoPagoToken();
                log.Info("Token MP: " + _token);
            }
            catch (Exception ex)
            {
                log.Error("Se produjo un error:" + ex);
                throw ex;
            }
        }
        #endregion

        #region Public Methods
        public List<MerchantOrderItem> RecuperarDatosPagosPorPreferenciaPago(string[] preferences)
        {
            var result = SDK.Get("/merchant_orders");
            var jsonResult = result.ToString();
            var elements = JsonConvert.DeserializeObject<MerchantOrderResponse>(jsonResult);
            return elements.Elements.Where(t => preferences.ToList().Any(j => j.Equals(t.Preference_Id))).ToList();
        }

        public PaymentSearchResponse RecuperarPagosTodos()
        {
            //rejected ; approved ; in_process
            var result = SDK.Get("/v1/payments/search");
            var jsonResult = result.ToString();
            var elements = JsonConvert.DeserializeObject<PaymentSearchResponse>(jsonResult);
            return elements;
        }

        public PaymentSearchResponse RecuperarPagoPorExternalReferenceId(int externalReferenceId)
        {
            try
            {
                log.Info($"RecuperarPagoPorExternalReferenceId() INICIO. ExternalReferenceId: {externalReferenceId}");
                string endpointUri = _mpApiEndpoint + "/v1/payments/search/?" + "access_token=" + _token + "&external_reference=" + externalReferenceId;
                log.Info($"Endpoint de consulta: {endpointUri}");
                var response = ServiceHelper.Get(endpointUri, null, log, 30, null);
                var result = response.Result;
                log.Info($"Código Respuesta: {response.Result.StatusCode}");
                if (result.IsSuccessStatusCode)
                {
                    var resultContent = result.Content.ReadAsStringAsync().Result;

                    if (!string.IsNullOrEmpty(resultContent.ToString()))
                    {
                        log.Info("Deserializando contenido de respuesta de la API.");
                        var elements = JsonConvert.DeserializeObject<PaymentSearchResponse>(resultContent);
                        return elements;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return null;
                }
                
            }
            catch (Exception ex)
            {
                log.Error("Se produjo una excepción durante la consulta: " + ex);
                return null;
            }
            finally
            {
                log.Info("RecuperarPagoPorExternalReferenceId() FIN.");
            }

        }
        #endregion

        #region Private Methods
        private void ReEvaluarYSetearMercadoPagoToken()
        {
            log.Info("ReEvaluarYSetearMercadoPagoToken() INICIO.");
            if ((SDK.AccessToken == null))
            {
                if (_isSandbox == true)
                {
                 
                    _token = ConfigurationManager.AppSettings["tokenMpagoSandBox"];
                    log.Info("MPToken sandbox:" + _token);
                }

                else
                {
                 
                    _token = ConfigurationManager.AppSettings["tokenMpagoProduccion"];
                    log.Info("MPToken producción:" + _token);
                }
                SDK.SetAccessToken(_token);
                log.Info("ReEvaluarYSetearMercadoPagoToken() FIN.");
            }
        }
        #endregion
    }
}
