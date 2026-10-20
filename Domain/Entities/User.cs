namespace Domain.Entities
{
    public class User : Person
    {
        public string Email { get; set; } = string.Empty;
        public string PasswordSalt { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        /// <summary>
        /// Comma-separated StaffPermissions codes. Admin ignores this (full access).
        /// </summary>
        public string Permissions { get; set; } = string.Empty;
    }
}
