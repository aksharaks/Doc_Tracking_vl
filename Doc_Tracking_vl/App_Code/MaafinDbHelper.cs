using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace Doc_Tracking_vl.App_Code
{
    public class MaafinDbHelper
    {
        public string conStr1;
        public string Connection()
        {
            //var client = new RestClient("https://serv.mactech.net.in/Maafin_API/");
            //var request = new RestRequest("ConnectionString");
            // var response = client.ExecuteAsync(request);
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:5000/");
            //client.BaseAddress = new Uri("https://serv.mactech.net.in/Maafin_API/");
            var response = client.GetAsync("ConnectionString").Result;
            var message = response.Content.ReadAsStringAsync().Result;
            conStr1 = message.ToString();
            return conStr1;

        }
    }
}
