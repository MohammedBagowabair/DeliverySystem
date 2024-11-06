namespace Domain.Entities
{
    public class User : Person
    {
        public string Email { get; set; } = string.Empty;             // Email used as the username for login
        public string? Password { get; set; } = string.Empty;
        public string PasswordSalt { get; set; } = string.Empty;           // Password for authentication
        public string PasswordHash { get; set; } = string.Empty;           // Password for authentication
        public string Role { get; set; } = string.Empty;              // Role to differentiate roles (e.g., Admin, Customer)
    }

}
