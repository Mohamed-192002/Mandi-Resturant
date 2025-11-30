using Core.Common;

namespace Core.Dtos.UserDto
{
    public class PrinterRegistrationRegisterDto
    {
        public string Name { get; set; }
        public string IpAddress { get; set; }
        public Guid UserId { get; set; }
        public List<CommonUserDrop> Users { get; set; } = [];
    }
}
