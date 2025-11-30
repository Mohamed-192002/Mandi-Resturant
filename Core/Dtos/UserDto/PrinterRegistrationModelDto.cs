using Core.Entities;

namespace Core.Dtos.UserDto
{
    public class PrinterRegistrationModelDto
    {
        public PrinterRegistrationRegisterDto PrinterRegistrationRegisterDto { get; set; } = new PrinterRegistrationRegisterDto();
        public IEnumerable<PrinterRegistration> PrinterRegistrationGetDtos { get; set; } = [];
    }
}
