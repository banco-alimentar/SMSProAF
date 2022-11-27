using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Xml;
using System.Net;

namespace SMSProAF
{
    public static class SendSMS
    {
        [FunctionName("SendSMS")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous,  "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("SendSMS Called");


            string ReqSecret, ReqMsisdn, ReqMessage,ReqChannel;
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            try
            { dynamic data = JsonConvert.DeserializeObject(requestBody);
                if (data == null)
                {
                    return new BadRequestObjectResult(@"no body was passed.");
                }

                ReqChannel = data?.channel ?? String.Empty;
                ReqSecret = data?.secret ?? String.Empty;
                ReqMsisdn = data?.msisdn ?? String.Empty;
                ReqMessage = data?.message ?? String.Empty;

            }
            catch (JsonReaderException exc)
            {
                return new BadRequestObjectResult($"Error parsing json body: {exc.Message}");
            }
            catch (Exception exc)
            {
                return new BadRequestObjectResult($"Exception processing request:{exc.Message}");
            }
            

            if (ReqChannel==String.Empty || ReqSecret== String.Empty || ReqMessage==String.Empty || ReqMsisdn==String.Empty)
            {
                return new BadRequestObjectResult(@"Body must include a json message with {""channel"":"""",""secret"":"""",""msisdn"":"""",""message"":""""}");
            }

            
            string Secret = Environment.GetEnvironmentVariable(ReqChannel, EnvironmentVariableTarget.Process);
            if (Secret!=ReqSecret)
            {
                log.LogInformation($"Incorrect Secret for channel {ReqChannel}");
                return new BadRequestObjectResult($"Incorrect Secret for channel {ReqChannel}");
            }

            log.LogInformation($"channel={ReqChannel} msisdn={ReqMsisdn} message={ReqMessage}");

            var _url = "https://smspro.nos.pt/smspro/smsprows.asmx";
            var _action = "https://smspro.nos.pt/smspro/smsprows.asmx?op=SendSMS";

            XmlDocument soapEnvelopeXml = CreateSoapEnvelope(ReqMsisdn, ReqMessage, Environment.GetEnvironmentVariable("SMSProUsername", EnvironmentVariableTarget.Process), Environment.GetEnvironmentVariable("SMSProPassword", EnvironmentVariableTarget.Process));
            HttpWebRequest webRequest = CreateWebRequest(_url, _action);
            InsertSoapEnvelopeIntoWebRequest(soapEnvelopeXml, webRequest);

            // begin async call to web request.
            IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);

            // suspend this thread until call is complete. You might want to
            // do something usefull here like update your UI.
            asyncResult.AsyncWaitHandle.WaitOne();

            // get the response from the completed web request.
            string soapResult;
            using (WebResponse webResponse = webRequest.EndGetResponse(asyncResult))
            {
                using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
                {
                    soapResult = rd.ReadToEnd();
                }
                log.LogInformation($"SMSProResult: {soapResult}");
            }


            return (ActionResult)new OkObjectResult($"SMS sent to {ReqMsisdn}");
        }


        private static HttpWebRequest CreateWebRequest(string url, string action)
        {
            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(url);
            webRequest.ContentType = "text/xml;charset=\"utf-8\"";
            webRequest.Accept = "text/xml";
            webRequest.Method = "POST";
            return webRequest;
        }

        private static XmlDocument CreateSoapEnvelope(string msisdn, string message,string username, string password)
        {
            XmlDocument soapEnvelopeDocument = new XmlDocument();
            soapEnvelopeDocument.LoadXml($@"<soap12:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap12=""http://www.w3.org/2003/05/soap-envelope""><soap12:Body><SendSMS xmlns=""http://www.outsystems.com""><TenantName>bancoalime</TenantName><strUsername>{username}</strUsername><strPassword>{password}</strPassword><MsisdnList>{msisdn}</MsisdnList><strMessage>{message}</strMessage></SendSMS></soap12:Body></soap12:Envelope>");
            return soapEnvelopeDocument;
        }

        private static void InsertSoapEnvelopeIntoWebRequest(XmlDocument soapEnvelopeXml, HttpWebRequest webRequest)
        {
            using (Stream stream = webRequest.GetRequestStream())
            {
                soapEnvelopeXml.Save(stream);
            }
        }
    }
}
