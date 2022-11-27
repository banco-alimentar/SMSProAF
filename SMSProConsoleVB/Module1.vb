Imports System.IO
Imports System.Net
Imports System.Text
Imports Newtonsoft.Json

Module Module1

    Sub Main()
        Dim JsonData As String = "{""channel"":""CampanhasVB6"",""secret"":""32fba9ebb40f42856df27b608b8cf7ac06a1ad44e8469c3c5326f226d1a9e712"",""msisdn"":925404518,""message"":""Mensqgem_Teste BA_JM""}"

        Dim data = Encoding.UTF8.GetBytes(JsonData)
        Dim result_post = SendRequest(New Uri("https://smspro.azurewebsites.net/api/SendSMS"), data, "application/json;charset=UTF-8", "POST")
        Console.WriteLine(result_post)


    End Sub

    Private Function SendRequest(uri As Uri, jsonDataBytes As Byte(), contentType As String, method As String) As String
        Try
            Dim req As WebRequest = WebRequest.Create(uri)
            req.ContentType = contentType
            req.Method = System.Net.WebRequestMethods.Http.Post
            req.ContentLength = jsonDataBytes.Length


            Dim stream = req.GetRequestStream()
            stream.Write(jsonDataBytes, 0, jsonDataBytes.Length)
            stream.Close()


            Dim response = req.GetResponse().GetResponseStream()

            Dim reader As New StreamReader(response)
            Dim res = reader.ReadToEnd()
            reader.Close()
            response.Close()
            Return res

        Catch ex As WebException
            Dim stream As New StreamReader(ex.Response.GetResponseStream())
            Dim res = stream.ReadToEnd()
            stream.Close()

            Return res

        End Try

    End Function
End Module
