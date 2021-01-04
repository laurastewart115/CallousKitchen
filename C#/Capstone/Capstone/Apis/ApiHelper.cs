using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace Capstone.Apis
{
    // Author: Peter
    // Client to handle all the apis
    public class ApiHelper
    {
        public HttpClient ApiClient { get; set; }

        /*public void InitializeClient()
        {
            System.IO.FileStream fs = System.IO.File.Create(@"C:\temp\test.txt");
            fs.Close();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            ApiClient = new HttpClient();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            ApiClient.DefaultRequestHeaders.Accept.Clear();
            ApiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
            System.Diagnostics.Debug.WriteLine("client init");

        }*/

        public ApiHelper()
        {
            ApiClient = new HttpClient();
            ApiClient.DefaultRequestHeaders.Accept.Clear();
            ApiClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            System.Diagnostics.Debug.WriteLine("client init");
        }
    }
}