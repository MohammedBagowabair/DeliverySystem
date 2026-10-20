namespace Domain.Entities
{
    /// <summary>
    /// Single-use password reset token. Store only a hash of the raw token.
    /// </summary>
    public class PasswordResetToken : BaseEntity
    {
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public string TokenHash { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public DateTime? UsedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
