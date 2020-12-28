using log4net;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Linq;

namespace ExternalServices.Common
{
    public class ServiceHelper
    {
        public static async Task<HttpResponseMessage> Get(String URL, String JsonParams, ILog log, int timeOut = 20, IDictionary<string,string> lstHeaders = null)
        {
            var response = new HttpResponseMessage();
            response.StatusCode = HttpStatusCode.NotFound;

            try
            {
                
                //ServicePointManager.Expect100Continue = true;
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                //ServicePointManager.ServerCertificateValidationCallback +=(sender, cert, chain, sslPolicyErrors) => true;

                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    if (lstHeaders != null) { lstHeaders.ToList().ForEach(z => client.DefaultRequestHeaders.Add(z.Key, z.Value)); }
                    client.Timeout = TimeSpan.FromSeconds(timeOut);

                     log.Info("Petición POST: " + URL);
                    log.Info("Contenido: " + JsonParams);

                    response = await client.GetAsync(new Uri(URL));
                }
            }
            catch (HttpRequestException rex)
            {
                log.Error(rex);
            }
            catch (TaskCanceledException tce)
            {
                log.Error("TaskCanceledException: " + tce);
                response.StatusCode = HttpStatusCode.RequestTimeout;
            }
            catch (TimeoutException tex)
            {
                log.Error("TimeoutException: " + tex);
                response.StatusCode = HttpStatusCode.RequestTimeout;
            }
            catch (Exception ex)
            {
                log.Error("Exception: " + ex);
            }

            return response;
        }
    }
}
