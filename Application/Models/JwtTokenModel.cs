namespace Application.Common.Models
{
    public class JwtTokenModel
    {
        public string Scope { get; set; }
        public string AccessToken { get; set; }
        public int ExpiresIn { get; set; }
        public string TokenType { get; set; }
        public DateTime IssueDate { get; set; }
    }
}
