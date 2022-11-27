using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SMSProConsoleCSharp
{
    class Program
    {
        static void Main(string[] args)
        {
            string JsonData = "{\"channel\":\"CampanhasVB6\",\"secret\":\"32fba9ebb40f42856df27b608b8cf7ac06a1ad44e8469c3c5326f226d1a9e712\",\"msisdn\":\"925404518\",\"message\":\"Mensqgem_Teste BA_JM\"}";
            string result = PostJson("https://smspro.azurewebsites.net/api/SendSMS", JsonData);
            Console.Write(result);
        }

        private static string PostJson(string url,string json ) {
            // create a request
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;
            request.Method = "POST";


            // turn our request string into a byte stream
            byte[] postBytes = Encoding.UTF8.GetBytes(json);

            // this is important - make sure you specify type this way
            request.ContentType = "application/json;";
            request.Accept = "application/json";
            request.ContentLength = postBytes.Length;
            Stream requestStream = request.GetRequestStream();

            // now send it
            requestStream.Write(postBytes, 0, postBytes.Length);
            requestStream.Close();

            // grab te response and print it out to the console along with the status code
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                string result;
                using (StreamReader rdr = new StreamReader(response.GetResponseStream()))
                {
                    result = rdr.ReadToEnd();
                }

                return result;
            }
            catch (WebException ex)
            {
                var resp = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();

                return resp;
            }
            

        }
    }
}
