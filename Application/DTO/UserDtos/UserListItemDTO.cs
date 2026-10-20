namespace Application.DTO.UserDtos
{
    public class UserListItemDTO
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public List<string> Permissions { get; set; } = new();
    }
}
