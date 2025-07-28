using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Dtos.RolesDto
{
    public class RolesModelDto
    {
        public RolesRegisterDto RolesRegisterDto { get; set; } = new RolesRegisterDto();
        public List<RolesGetDto> RolesGetDtos { get; set; } = new List<RolesGetDto>();
    }
}
