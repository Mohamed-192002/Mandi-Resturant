using System.Net;
using System.Text;

namespace SiteFront.Services
{
    public static class ApiHelper
    {
        public static async Task SendToApi(string pdfFilePath, string printerName, string localIp)
        {
            byte[] fileBytes = await File.ReadAllBytesAsync(pdfFilePath);
            string base64 = Convert.ToBase64String(fileBytes);
            string data = $"{base64}|{printerName}";

            // 🔹 Get the local IP address dynamically
          //  string localIp = GetLocalIPAddress();

            using (var client = new HttpClient())
            {
                var content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(new { Data = data }),
                    Encoding.UTF8,
                    "application/json"
                );

                // 🔹 Send request to the local agent using the current device IP
                string url = $"http://{localIp}:5555/api/Print";
                var response = await client.PostAsync(url, content);
                Console.WriteLine(await response.Content.ReadAsStringAsync());
            }
        }

        // 🧩 Helper to get the current machine's IPv4 address
        //private static string GetLocalIPAddress()
        //{
        //    string localIP = "127.0.0.1"; // fallback
        //    var host = Dns.GetHostEntry(Dns.GetHostName());
        //    foreach (var ip in host.AddressList)
        //    {
        //        if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        //        {
        //            localIP = ip.ToString();
        //            break;
        //        }
        //    }
        //    return localIP;
        //}

    }
}
