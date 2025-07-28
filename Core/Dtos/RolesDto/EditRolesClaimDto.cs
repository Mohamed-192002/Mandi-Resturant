namespace Core.Dtos.RolesDto
{
    public class EditRolesClaimDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }

        public List<ClaimViewModel>? Claims { get; set; }
    }
}
