namespace SiteFront.Services
{
    public static class ApiHelper
    {
        public static async Task SendToApi(string pdfFilePath, string printerName)
        {
            byte[] fileBytes = await File.ReadAllBytesAsync(pdfFilePath);
            string base64 = Convert.ToBase64String(fileBytes);
            string data = $"{base64}|{printerName}";

            using (var client = new HttpClient())
            {
                var content = new StringContent(
                    System.Text.Json.JsonSerializer.Serialize(new { Data = data }),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                var response = await client.PostAsync("http://localhost:5555/api/Print", content);
                Console.WriteLine(await response.Content.ReadAsStringAsync());
            }
        }

    }
}
