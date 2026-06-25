using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace IDEPython.Services
{
    public class ApiService
    {
        private readonly HttpClient client;
        public string? Token { get; set; }

        public ApiService()
        {
            client = new HttpClient();

            client.BaseAddress =
                new Uri("http://138.2.235.169/dsw/public/api");
        }

        public async Task<string> GetAsync(string endpoint)
        {
            if (!string.IsNullOrEmpty(Token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue(
                        "Bearer",
                        Token
                    );
            }

            HttpResponseMessage response =
                await client.GetAsync(client.BaseAddress + endpoint);

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> PostAsync(string endpoint, object datos)
        {
            if (!string.IsNullOrEmpty(Token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue(
                        "Bearer",
                        Token
                    );
            }

            string json = JsonSerializer.Serialize(datos);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json"
            );

                HttpResponseMessage response =
                    await client.PostAsync(client.BaseAddress + endpoint, content);

            return await response.Content.ReadAsStringAsync();
        }
    }
}