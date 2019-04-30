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

            string ReqSecret, ReqMsisdn, ReqMessage;

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            ReqSecret = data?.secret ?? "";
            ReqMsisdn = data?.msisdn ?? "";
            ReqMessage = data?.message ?? "";

            string Secret = Environment.GetEnvironmentVariable("secret", EnvironmentVariableTarget.Process);
            if (Secret!=ReqSecret)
            {
                log.LogInformation("Incorrect Secret");
                return new BadRequestObjectResult("Incorrect Secret");
            }

            log.LogInformation($"msisdn={ReqMsisdn} message={ReqMessage}");

            var _url = "https://smspro.nos.pt/smspro/smsprows.asmx";
            var _action = "https://smspro.nos.pt/smspro/smsprows.asmx?op=SendSMS";

            XmlDocument soapEnvelopeXml = CreateSoapEnvelope(ReqMsisdn,ReqMessage, Environment.GetEnvironmentVariable("SMSProUsername", EnvironmentVariableTarget.Process), Environment.GetEnvironmentVariable("SMSProPassword", EnvironmentVariableTarget.Process));
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


            return data != null
                ? (ActionResult)new OkObjectResult($"SMS sent to {ReqMsisdn}")
                : new BadRequestObjectResult(@"Body must include a json message with {""secret"":"""",""msisdn"":"""",""message"":""""}");
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
