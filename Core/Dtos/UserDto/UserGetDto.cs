namespace Core.Dtos.UserDto
{
    public class UserGetDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();

    }
}
