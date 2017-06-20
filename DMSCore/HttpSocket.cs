using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Net.Http;

using System.Threading.Tasks;

namespace MonitorCore
{
    class HttpSocket
    {
        private HttpClient client = null;
        private string BASE_ADDRESS = "";
        private int port=0;
        public HttpSocket(string BASE_ADDRESS,int port)
        {
            this.BASE_ADDRESS = BASE_ADDRESS;
            this.port = port;
            client = new HttpClient();
            client.MaxResponseContentBufferSize = 25600;

            client.DefaultRequestHeaders.ExpectContinue = false;


            Uri uriBase = new Uri(BASE_ADDRESS);
            Uri serviceReq = new Uri(uriBase,"/alarm?UID=123456789012345678901234&TIME=20170624050403");
            //HttpContent content = new StringContent(@"{ ""value"": ""test""}");
            //读取视频文件
            byte[] File_Data=File.ReadAllBytes("F:\\aa.csv");

            //赋值
            HttpContent content = new ByteArrayContent(File_Data);
            content.Headers.Add("Content-Type", "video/mpeg4");
            
            //发送
            HttpResponseMessage response=client.PostAsync(serviceReq, content).Result;
            
            if (response.IsSuccessStatusCode)
            {
                Task<string> t = response.Content.ReadAsStringAsync();
                //return t.Result;
            }

            /*client.SendAsync(new HttpRequestMessage
            {
                Method = new HttpMethod("HEAD"),
                RequestUri = new Uri(BASE_ADDRESS + "/")
            })
            .Result.EnsureSuccessStatusCode();*/


            //String url = "http://passport.cnblogs.com/login.aspx";
            //HttpResponseMessage response = client.GetAsync(new Uri(url)).Result;
            //String result = response.Content.ReadAsStringAsync().Result;

            //String username = "hi_amos";
            //String password = "密码";
        }

    }
}
