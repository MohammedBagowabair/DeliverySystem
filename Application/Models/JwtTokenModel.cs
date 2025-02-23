using System.ComponentModel;

namespace Application.Common.Models
{
    public class JwtTokenModel
    {
        [Description("Jwt Access Token")]
        public string AccessToken { get; set; }
        [Description("In seconds")]
        public int ExpiresIn { get; set; }
        [Description("Token Type, Bearer")]
        public string TokenType { get; set; }
    }
}
