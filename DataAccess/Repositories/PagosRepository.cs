using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using DataAccess.Models;
using System.Configuration;

namespace DataAccess.Repositories
{
    public enum EstadosPagos
    {
        Creado = 0,
        Aprobado = 1,
        Rechazado = 2,
        Demorado = 3

    }

    public class PagosRepository : BaseRepository
    {

        public PagosRepository(string connString) : base(connString)
        {

        }

        public List<BLLPago> RecuperarPagosIndeterminados()
        {
          
            SqlDataAdapter daPagosIndeterminados = new SqlDataAdapter("", Connection);
            DataSet dsPagosIndeterminados = new DataSet();

            List<BLLPago> pagosIndeterminados = new List<BLLPago>();
            string sQuery = null;
            try
            {

                int cantHoras =  (-1) * Convert.ToInt32(ConfigurationManager.AppSettings["cantHorasTarea"]);
                log.Info($"Cant. Minutos Tarea:{cantHoras}");
                sQuery = " SELECT " +
                        " Pagos.IdPago, Pagos.Estado, Pagos.FechaCreacion, Pagos.NumeroFactura, Pagos.Importe, Pagos.Preference, Pagos.Collection, Pagos.MerchantOrder, Pagos.IdSocio, Pagos.IdConexion " +
                        " FROM Pagos " +
                        " WHERE " +
                        " Pagos.Estado IN ('" + (int)EstadosPagos.Creado + "'," + "'" + (int)EstadosPagos.Demorado + "') AND " +
                        " Pagos.FechaCreacion <= DATEADD(MINUTE, " + cantHoras + " , GETDATE())";

                log.Info($"Consulta SQL de recuperación de transacciones: {sQuery}");

                Command.CommandText = sQuery;
                Command.Parameters.Clear();
                //Command.Parameters.Add("@cantHoras", cantHoras);
                Connection.Open();
                daPagosIndeterminados.SelectCommand = Command;
                daPagosIndeterminados.Fill(dsPagosIndeterminados);
                daPagosIndeterminados.Dispose();
                Command.Dispose();
                Connection.Close();


                if (dsPagosIndeterminados.Tables != null &&
                        dsPagosIndeterminados.Tables.Count == 1)
                {
                    foreach (DataRow dr in dsPagosIndeterminados.Tables[0].Rows)
                    {
                        pagosIndeterminados.Add(new BLLPago()
                        {
                            IdPago = Convert.ToInt32(dr["IdPago"]),
                            Estado = Convert.ToInt32(dr["Estado"]),
                            FechaCreacion = Convert.ToDateTime(dr["FechaCreacion"]),
                            NumeroFactura = (dr["NumeroFactura"] != DBNull.Value) ? dr["NumeroFactura"].ToString() : string.Empty,
                            Importe = Convert.ToDecimal(dr["Importe"]),
                            Preference = (dr["Preference"] != DBNull.Value) ? dr["Preference"].ToString() : string.Empty,
                            Collection = (dr["Collection"] != DBNull.Value) ? dr["Collection"].ToString() : string.Empty,
                            MerchantOrder = (dr["MerchantOrder"] != DBNull.Value) ? dr["MerchantOrder"].ToString() : string.Empty,
                            IdSocio = (dr["IdSocio"] != DBNull.Value) ? Convert.ToInt32(dr["IdSocio"]) : default(Int32?),
                            IdConexion = (dr["IdConexion"] != DBNull.Value) ? Convert.ToInt32(dr["IdConexion"]) : default(Int32?),
                        }); ;
                    }

                }

                return pagosIndeterminados;
              }
            catch (Exception ex)
            {
                if (Connection != null) Connection.Close();
                pagosIndeterminados = null;
                Command.Dispose();
                daPagosIndeterminados.Dispose();
                sQuery = null;
                return pagosIndeterminados;
            }

            finally
            {
                Command.Dispose();
                sQuery = null;
            }
        }
        public void ActualizarEstadoPago(int idPago, int estadoPago, string collection, string merchantOrderId)
        {
            if (!Enum.IsDefined(typeof(EstadosPagos), estadoPago))
            {
                log.Error($"El estado {estadoPago} no se corresponde con ningun estado posible");
                throw new Exception("El estado no se corresponde con ningun estado posible");
            }
            log.Info($"ActualizarEstadoPago(). Parametros: idPago:{idPago}, estadoPago:{estadoPago}");
            string sQuery = null;
            try
            {
                Command.Parameters.Clear();
                sQuery = " UPDATE Pagos SET Pagos.Estado = @estado, FechaActualizacion = GETDATE(), ActualizadoPor = 'Tarea Programada' ";
                if (!string.IsNullOrEmpty(merchantOrderId))
                {
                    sQuery += " , MerchantOrder = @merchantOrderId ";
                    Command.Parameters.Add("@merchantOrderId", merchantOrderId);
                }
                if (!string.IsNullOrEmpty(collection) && Convert.ToInt64(collection)>0)
                {
                    sQuery += " , Collection = @collection ";
                    Command.Parameters.Add("@collection", collection);
                }
                sQuery += " WHERE Pagos.idPago = @idPago ";

                Command.CommandText = sQuery;
                Command.Parameters.Add("@estado", estadoPago);
                Command.Parameters.Add("@idPago", idPago);
                Connection.Open();
                Command.ExecuteNonQuery();
                Command.Dispose();
                Connection.Close();
            }
            catch (Exception ex)
            {
                log.Error("Se produjo una excepcion:" + ex);
                if (Connection != null) Connection.Close();
                Command.Dispose();
                sQuery = null;
            }

            finally
            {
                Command.Dispose();
                sQuery = null;
            }
        }
    }
}