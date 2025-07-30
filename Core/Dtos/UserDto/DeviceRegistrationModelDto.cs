using Core.Entities;

namespace Core.Dtos.UserDto
{
    public class DeviceRegistrationModelDto
    {
        public DeviceRegistrationRegisterDto DeviceRegistrationRegisterDto { get; set; } = new DeviceRegistrationRegisterDto();
        public IEnumerable<DeviceRegistration> DeviceRegistrationGetDtos { get; set; } = [];
    }
}
