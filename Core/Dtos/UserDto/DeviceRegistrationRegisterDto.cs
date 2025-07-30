using Core.Common;
namespace Core.Dtos.UserDto
{
    public class DeviceRegistrationRegisterDto
    {
        public string AccessCode { get; set; }
        public Guid UserId { get; set; }
        public List<CommonUserDrop> Users { get; set; } = [];
    }
}