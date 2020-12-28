using DataAccess.Models;
using DataAccess.Repositories;
using ExternalServices;
using ExternalServices.ServicesResponses;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace MercadoPagoScheduledTasks
{
    class Program
    {


        private enum ScheduledTasks
        {
            ConsultarEstadoTransacciones = 1
        }
        static void Main(string[] args)
        {
            log4net.Config.BasicConfigurator.Configure();
            log4net.ILog log = log4net.LogManager.GetLogger(typeof(Program));

            try
            {
                log.Info("MercadoPagoScheduledTasks START");
                ScheduledTasks scheduledTask;
                object[] arrArgsParams = null;
                string appConn = ConfigurationManager.ConnectionStrings["appConn"].ToString();
                #region ValidateArguments
                //Check number of Arguments
                if (args.Length < 1)
                {
                    log.Error("Cantidad de argumentos invalidos");
                    throw new Exception("Error. Cantidad de argumentos invalidos.");
                }
                //Validate Task
                try
                {
                    scheduledTask = (ScheduledTasks)Convert.ToInt32(args[0]);
                }
                catch (Exception ex)
                {
                    log.Error("Tarea invalida", ex);
                    throw new Exception("Error. Argumentos invalidos: " + ex.Message + ".");
                }
                //Get additional parameters. 
                if (args.Length > 1)
                {
                    arrArgsParams = new object[args.Length - 1];
                    for (int idx = 0; idx < arrArgsParams.Length; idx++)
                        arrArgsParams[idx] = args[idx + 1];
                }
                #endregion ValidateArguments


                #region Call Scheduled Task

                //scheduledTask = (ScheduledTasks)1;

                try
                {

                    switch (scheduledTask)
                    {

                        case ScheduledTasks.ConsultarEstadoTransacciones:
                            ExternalServices.MercadoPago mpServices = new ExternalServices.MercadoPago();
                            PagosRepository pagos = new PagosRepository(appConn);
                            log.Info("Recuperando pagos con estados indeterminados...");
                            List<BLLPago> pagosIndeterminados = pagos.RecuperarPagosIndeterminados();
                            int[] pagosAVerificar = pagosIndeterminados?.Select(t => t.IdPago)?.ToArray();
                            log.Info($"Total Pagos Recuperados:{(pagosAVerificar?.Count() ?? 0)} ");

                            log.Info("Verificando pagos en la API de mercado pago...");
                            foreach (int idPago in pagosAVerificar)
                            {
                                log.Info($"Consultando pago con idPago: {idPago} en API Mercado Pago.");
                                PaymentSearchResponse elements = mpServices.RecuperarPagoPorExternalReferenceId(idPago);

                                if (elements != null &&
                                        elements.results != null &&
                                            elements.results.Count > 0)
                                {
                                    if (elements.results.First().Status.Equals(MercadoPago.MP_APPROVED_STATUS, StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        //APROBAMOS EN NUESTRA PLATAFORMA.
                                        log.Info($"El pago con idPago {idPago} se encontraba aprobado en mercado pago.");
                                        log.Info($"Aprobando pago {idPago} en plataforma.");
                                        pagos.ActualizarEstadoPago(idPago, (int)EstadosPagos.Aprobado, elements.results.First().Id.ToString(), elements.results.First().Order.Id);
                                        log.Info($"Pago {idPago} aprobado en plataforma.");
                                    }
                                    else if (elements.results.First().Status.Equals(MercadoPago.MP_REJECTED_STATUS, StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        //RECHAZAMOS EN NUESTRA PLATAFORMA.
                                        log.Info($"El pago con idPago {idPago} se encontraba rechazado en mercado pago.");
                                        log.Info($"Rechazando pago {idPago} en plataforma.");
                                        pagos.ActualizarEstadoPago(idPago, (int)EstadosPagos.Rechazado, elements.results.First().Id.ToString(), elements.results.First().Order.Id);
                                        log.Info($"Pago {idPago} rechazado en plataforma.");
                                    }
                                    else
                                    {
                                        log.Info($"El pago con idPago {idPago} se encontraba en estado {elements.results.First().Status} en mercado pago, el cual representa un estado indeterminado. No actualizaremos la transacción.");
                                        //NO HACEMOS NADA, LA RECONSULTAMOS A FUTURO.
                                    }
                                }
                                else
                                {
                                    log.Info($"El pago con idPago {idPago} no existía en mercado pago.");
                                    //RECHAZAMOS EN NUESTRA PLATAFORMA
                                    log.Info($"Rechazando pago {idPago} en plataforma.");
                                    pagos.ActualizarEstadoPago(idPago, (int)EstadosPagos.Rechazado, null, null);
                                    log.Info($"Pago {idPago} rechazado en plataforma.");
                                }

                           }

                            log.Info("MercadoPagoScheduledTasks END.");
                            break;

                        default:
                            throw new Exception("Tarea inválida (" + scheduledTask.ToString() + ")");
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Error en llamado de tarea programada. " + ex.Message);
                    throw ex;
                }
                #endregion Call Scheduled Task
            }

            catch (Exception ex)
            {
                log.Error("Ha ocurrido un error", ex);
            }
            finally
            {
                log.Info("MercadoPagoScheduledTasks END");
            }
        }
    }
}
