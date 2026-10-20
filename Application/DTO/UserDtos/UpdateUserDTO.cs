namespace Application.DTO.UserDtos
{
    public class UpdateUserDTO : PersonDTO
    {
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new();
    }
}
