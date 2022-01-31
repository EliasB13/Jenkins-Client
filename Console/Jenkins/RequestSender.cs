using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Jenkins
{
    public class RequestSender
    {
        private readonly HttpClient httpClient = new();
        private readonly string userName;
        private readonly string token;

        public RequestSender(string userName, string token)
        {
            this.userName = userName;
            this.token = token;
        }

        public async Task<HttpResponseMessage> GetAsync(string uri, HttpMethod method)
        {
            var authenticationString = $"{userName}:{token}";
            var encodedAuthString = Convert.ToBase64String(Encoding.ASCII.GetBytes(authenticationString));

            var requestMessage = new HttpRequestMessage(method, uri);
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", encodedAuthString);

            return await httpClient.SendAsync(requestMessage);
        }
    }
}
