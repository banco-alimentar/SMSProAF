using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using NUnit.Framework;

namespace SMSProAF
{
    [TestFixture]
    class TestSendSMS
    {
        string functionUrl = "http://localhost:7071/api/SendSMS";

        [Test]
        public void NoBody()
        {
            using (var client = new HttpClient())
            {
                var response = client.PostAsync(functionUrl,new StringContent(String.Empty, Encoding.UTF8, "application/json"));
            }
            //Program pobj = new Program();
            //li = pobj.AllUsers();
            //foreach (var x in li)
            //{
            //    Assert.IsNotNull(x.id);
            //    Assert.IsNotNull(x.Name);
            //    Assert.IsNotNull(x.salary);
            //    Assert.IsNotNull(x.Geneder);
            //}
        }
    }


}
