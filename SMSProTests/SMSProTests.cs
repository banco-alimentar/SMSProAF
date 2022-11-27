using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;
using SMSProAF;
using System.Net;

namespace SMSProTests
{
    public class SMSProTests
    {
        private readonly ILogger logger = TestFactory.CreateLogger();

        [Fact]
        public async void SendSMS_With_No_Body_should_return_BadRequestObjectResult()
        {
            var request = TestFactory.CreateHttpRequest(string.Empty,string.Empty,string.Empty);
            var response = await SendSMS.Run(request, logger);
            Assert.Equal(typeof(BadRequestObjectResult), response.GetType());
        }

        [Fact]
        public async void SendSMS_With_Wrong_Json_Body_should_return_BadRequestObjectResult()
        {
            var request = TestFactory.CreateHttpRequest(string.Empty, string.Empty,"{\",\",");
            var response = await SendSMS.Run(request, logger);
            Assert.Equal(typeof(BadRequestObjectResult), response.GetType());
        }

        [Fact]
        public async void SendSMS_With_Missing_Parameter_Channel_In_Json()
        {
            var request = TestFactory.CreateHttpRequest(string.Empty, string.Empty, "{\"other\":\"CampanhasVB6\",\"secret\":\"2\",\"msisdn\":\"NUMEROTELEMOVEL\",\"message\":\"Mensagem a enviar\"}");
            var response = await SendSMS.Run(request, logger);
            Assert.Equal(typeof(BadRequestObjectResult), response.GetType());
        }

        [Fact]
        public async void SendSMS_With_Missing_Parameter_secret_In_Json()
        {
            var request = TestFactory.CreateHttpRequest(string.Empty, string.Empty, "{\"channel\":\"CampanhasVB6\",\"msisdn\":\"NUMEROTELEMOVEL\",\"message\":\"Mensagem a enviar\"}");
            var response = await SendSMS.Run(request, logger);
            Assert.Equal(typeof(BadRequestObjectResult), response.GetType());
        }

        [Fact]
        public async void SendSMS_With_Wrong_Secret_In_Json()
        {
            var request = TestFactory.CreateHttpRequest(string.Empty, string.Empty, "{\"channel\":\"CampanhasVB6\",\"secret\":\"Wrong!Secret!Test\",\"msisdn\":\"NUMEROTELEMOVEL\",\"message\":\"Mensagem a enviar\"}");
            var response = await SendSMS.Run(request, logger);
            Assert.Equal(typeof(BadRequestObjectResult), response.GetType());
        }
        //[Theory]
        //[MemberData(nameof(TestFactory.Data), MemberType = typeof(TestFactory))]
        //public async void Http_trigger_should_return_known_string_from_member_data(string queryStringKey, string queryStringValue)
        //{
        //    var request = TestFactory.CreateHttpRequest(queryStringKey, queryStringValue);
        //    var response = (OkObjectResult)await SendSMS.Run(request, logger);
        //    Assert.Equal($"Hello, {queryStringValue}", response.Value);
        //}

        //[Fact]
        //public void Timer_should_log_message()
        //{
        //    var logger = (ListLogger)TestFactory.CreateLogger(LoggerTypes.List);
        //    SendSMS.Run(null, logger);
        //    var msg = logger.Logs[0];
        //    Assert.Contains("C# Timer trigger function executed at", msg);
        //}
    }
}