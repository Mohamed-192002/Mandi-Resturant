using System.ComponentModel.DataAnnotations;

namespace Core.Dtos.RolesDto
{
    public class RolesRegisterDto
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "هذا الحقل مطلوب")]
        public string Name { get; set; } = "";
    }
}
