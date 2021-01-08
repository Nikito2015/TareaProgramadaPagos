using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using log4net;
using Newtonsoft.Json;
using ExternalServices.ServicesResponses;
using ExternalServices.Common;
using System.Net.Http;
using System.Net;
using PlusPagosConnector.NetFramework;
using PlusPagosConnector.NetFramework.Models;
using System.Security.Cryptography;
using DataAccess.Models;
using DataAccess;
using DataAccess.Repositories;

namespace ExternalServices
{
    /// <summary>
    /// Contiene los métodos de consulta a Macro Click
    /// </summary>
    public class MacroClick
    {
        #region Private Members
        private bool _isSandbox;
        private string _frase;
        private string _guid;
        private string _token;
        private ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #endregion

        #region Public Methods
        public void ConsultarPagosMacro(List<BLLPago> pagosIndeterminadosMacro)
        {
            log.Info("ConsultarPagosMacro() INICIO.");
            PPConnector ppConnector;
            Response status;
            var model = new AuthenticationModel();
            string appConn = ConfigurationManager.ConnectionStrings["appConn"].ToString();
            PagosRepository pagos = new PagosRepository(appConn);

            try
            {
                System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
                _isSandbox = Convert.ToBoolean(ConfigurationManager.AppSettings["isSandbox"].ToString());
                log.Info("ConsultarPagosMacro() - IsSandbox: " + _isSandbox);
                if (_isSandbox == true)
                {
                    ppConnector = new PPConnector(AmbienteHelper.Ambiente.SANDBOX);
                    status = ppConnector.HealthCheck();

                    log.Info("ConsultarPagosMacro() Código Respuesta al conectarnos a la api de Macro: " + status.code);
                    if (status.code == 200)
                    {
                        model = new AuthenticationModel()
                        {
                            frase = ConfigurationManager.AppSettings["fraseMacroSANDBOX"],
                            guid = ConfigurationManager.AppSettings["guidMacroSANDBOX"]
                        };
                    }
                    else
                    {
                        log.Info("ConsultarPagosMacro() ERROR " + status.code + " - " + status.message);
                        return;
                    }
                }
                else
                {
                    ppConnector = new PPConnector(AmbienteHelper.Ambiente.PRODUCTION);
                    status = ppConnector.HealthCheck();
                    log.Info("ConsultarPagosMacro() Código Respuesta al conectarnos a la api de Macro: " + status.code);

                    if (status.code == 200)
                    {
                        model = new AuthenticationModel()
                        {
                            frase = ConfigurationManager.AppSettings["fraseMacroPRODUCCION"],
                            guid = ConfigurationManager.AppSettings["guidMacroPRODUCCION"]
                        };
                    }
                    else
                    {
                        log.Info("ConsultarPagosMacro() ERROR " + status.code + " - " + status.message);
                        return;
                    }
                }

                log.Info("ConsultarPagosMacro() Obtenemos token.");
                var access_token = ppConnector.GetAuthenticationToken(model);

                log.Info("ConsultarPagosMacro() - Código Respuesta al obtener token: " + access_token.code);
                if (access_token.code == 200)
                {
                    log.Info("ConsultarPagosMacro() - " + access_token.message);
                    _token = access_token.data;

                    log.Info("ConsultarPagosMacro() - Obtenemos pagos por TransaccionComercioIdMacro.");
                    
                    foreach (var datos in pagosIndeterminadosMacro)
                    {
                        log.Info("ConsultarPagosMacro() - Consulta API Macro - Transacción: " + datos.TransaccionComercioIdMacro);
                        Response transaccion = ppConnector.GetTransactionByTxComercioId(_token, datos.TransaccionComercioIdMacro);

                        log.Info("ConsultarPagosMacro() - Código Respuesta al obtener transacción: " + transaccion.code);
                        if (transaccion.code == 200 && transaccion.data.Length > 0)
                        {
                            log.Info("ConsultarPagosMacro() - Deserializando contenido de respuesta de la API.");
                            PaymentMacroResponse ResponseMacro = JsonConvert.DeserializeObject<PaymentMacroResponse>(transaccion.data);

                            log.Info("ConsultarPagosMacro() - Realizamos la actualización del estado de la transacción.");
                            
                            if (ResponseMacro.Estado.ToUpper() == "REALIZADA")
                            {
                                log.Info($"El pago con idPago {datos.IdPago} se encontraba aprobado en Macro.");
                                log.Info($"Aprobando pago {datos.IdPago} en plataforma.");
                                pagos.ActualizarEstadoPagoMacro(datos.IdPago, (int)EstadosPagos.Aprobado, ResponseMacro.TransaccionId);
                                log.Info($"Pago {datos.IdPago} aprobado en plataforma.");
                            }
                            else if (ResponseMacro.Estado.ToUpper() == "RECHAZADA")
                            {
                                log.Info($"El pago con idPago {datos.IdPago} se encontraba rechazado en Macro.");
                                log.Info($"Rechazando pago {datos.IdPago} en plataforma.");
                                pagos.ActualizarEstadoPagoMacro(datos.IdPago, (int)EstadosPagos.Rechazado, ResponseMacro.TransaccionId);
                                log.Info($"Pago {datos.IdPago} rechazado en plataforma.");
                            }
                            else
                            {
                                log.Info($"El pago con idPago {datos.IdPago} se encontraba en estado {ResponseMacro.Estado} en Macro, el cual representa un estado indeterminado. Se realizará la consulta a futuro.");                  
                            }
                        }
                        else
                        {
                            log.Info("ConsultarPagosMacro() ERROR " + status.code + " - " + status.message);
                            log.Info($"El pago con idPago {datos.IdPago} no existía en Macro.");                     
                            log.Info($"Rechazando pago {datos.IdPago} en plataforma.");
                            pagos.ActualizarEstadoPagoMacro(datos.IdPago, (int)EstadosPagos.Rechazado, null);
                            log.Info($"Pago {datos.IdPago} rechazado en plataforma.");
                        }
                    }
                }
                else
                {
                    log.Info("ConsultarPagosMacro() ERROR " + status.code + " - " + status.message);
                    return;
                }
            }
            catch (Exception ex)
            {
                log.Error("Se produjo una excepción durante la consulta: " + ex);               
            }
            finally
            {
                log.Info("ConsultarPagosMacro() FIN.");
            }
        }
        #endregion
    }
}
