namespace SiteFront.Models
{
    public class WhatsAppMessageRequest
    {
        public string session_id { get; set; }
        public string receiver { get; set; }
        public string text { get; set; }
    }
}
