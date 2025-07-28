using System.Management;

namespace SiteFront.Services
{
    public class DeviceIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _allowedDeviceId;

        public DeviceIdMiddleware(RequestDelegate next, string allowedDeviceId)
        {
            _next = next;
            _allowedDeviceId = allowedDeviceId;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string deviceId = GetDeviceId();
            if (deviceId != _allowedDeviceId)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "text/plain; charset=utf-8";
                await context.Response.WriteAsync("ليس لك الحق في تشغيل هذا البرنامج تواصل مع الشركة إذا كنت تريد نسخة");
                return;
            }

            await _next(context);
        }

        private string GetDeviceId()
        {
            string processorId = GetProcessorId();
            string biosSerialNumber = GetBiosSerialNumber();

            return $"{processorId}-{biosSerialNumber}";
        }

        private string GetProcessorId()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT ProcessorId FROM Win32_Processor");
                foreach (ManagementObject obj in searcher.Get())
                {
                    return obj["ProcessorId"].ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving Processor ID: " + ex.Message);
            }
            return string.Empty;
        }

        private string GetBiosSerialNumber()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BIOS");
                foreach (ManagementObject obj in searcher.Get())
                {
                    return obj["SerialNumber"].ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error retrieving BIOS Serial Number: " + ex.Message);
            }
            return string.Empty;
        }
    }
}
