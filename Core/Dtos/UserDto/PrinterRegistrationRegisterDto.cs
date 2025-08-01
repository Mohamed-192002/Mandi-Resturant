using Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Dtos.UserDto
{
    public class PrinterRegistrationRegisterDto
    {
        public string Name { get; set; }
        public Guid UserId { get; set; }
        public List<CommonUserDrop> Users { get; set; } = [];
    }
}
