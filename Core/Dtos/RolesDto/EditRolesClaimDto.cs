using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Dtos.RolesDto
{
 public   class EditRolesClaimDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }

        public List<ClaimViewModel>? Claims { get; set; }
    }
}
