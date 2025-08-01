using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Dtos.UserDto
{
    public class PrinterRegistrationModelDto
    {
        public PrinterRegistrationRegisterDto PrinterRegistrationRegisterDto { get; set; } = new PrinterRegistrationRegisterDto();
        public IEnumerable<PrinterRegistration>  PrinterRegistrationGetDtos { get; set; } = [];
    }
}
