using Newtonsoft.Json;
using SiteFront.Models;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace SiteFront.Services
{
    public class WhatsAppService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public WhatsAppService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<bool> SendMessageAsync(string phoneNumber, string message)
        {
            var apiUrl = $"{_configuration["WhatsAppApi:BaseUrl"]}/message/text/send";
            var sessionId = _configuration["WhatsAppApi:SessionId"];
            var token = _configuration["WhatsAppApi:Token"];
            var countryKey = _configuration["WhatsAppApi:CountryKey"];

            var requestBody = new WhatsAppMessageRequest
            {
                session_id = sessionId,
                receiver = countryKey + phoneNumber,
                text = message
            };

            var json = JsonConvert.SerializeObject(requestBody);
            var content = new StringContent(json, new MediaTypeWithQualityHeaderValue("application/json"));

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            _httpClient.DefaultRequestHeaders.Accept.Add( new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.PostAsync(apiUrl, content);
            return response.IsSuccessStatusCode;
        }
    }
}
