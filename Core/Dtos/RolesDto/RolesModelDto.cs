namespace Core.Dtos.RolesDto
{
    public class RolesModelDto
    {
        public RolesRegisterDto RolesRegisterDto { get; set; } = new RolesRegisterDto();
        public List<RolesGetDto> RolesGetDtos { get; set; } = new List<RolesGetDto>();
    }
}
