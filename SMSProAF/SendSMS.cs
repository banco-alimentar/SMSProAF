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
            log.LogInformation("C# HTTP trigger function processed a request.");

            string secret, msisdn, message;

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            secret = data?.secret ?? "";
            msisdn = data?.msisdn ?? "";
            message = data?.message ?? "";


            var _url = "https://smspro.nos.pt/smspro/smsprows.asmx";
            var _action = "https://smspro.nos.pt/smspro/smsprows.asmx?op=SendSMS";

            XmlDocument soapEnvelopeXml = CreateSoapEnvelope();
            HttpWebRequest webRequest = CreateWebRequest(_url, _action);
            InsertSoapEnvelopeIntoWebRequest(soapEnvelopeXml, webRequest);

            //// begin async call to web request.
            //IAsyncResult asyncResult = webRequest.BeginGetResponse(null, null);

            //// suspend this thread until call is complete. You might want to
            //// do something usefull here like update your UI.
            //asyncResult.AsyncWaitHandle.WaitOne();

            //// get the response from the completed web request.
            //string soapResult;
            //using (WebResponse webResponse = webRequest.EndGetResponse(asyncResult))
            //{
            //    using (StreamReader rd = new StreamReader(webResponse.GetResponseStream()))
            //    {
            //        soapResult = rd.ReadToEnd();
            //    }
            //    Console.Write(soapResult);
            //}


            return data != null
                ? (ActionResult)new OkObjectResult($"SMS sent to {msisdn}")
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

        private static XmlDocument CreateSoapEnvelope()
        {
            XmlDocument soapEnvelopeDocument = new XmlDocument();
            soapEnvelopeDocument.LoadXml(@"<soap12:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:soap12=""http://www.w3.org/2003/05/soap-envelope""><soap12:Body><SendSMS xmlns=""http://www.outsystems.com""><TenantName>bancoalime</TenantName><strUsername>BA</strUsername><strPassword>DeNDH</strPassword><MsisdnList>925404518</MsisdnList><strMessage>a message</strMessage></SendSMS></soap12:Body></soap12:Envelope>");
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
