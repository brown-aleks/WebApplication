//  https://memorycrypt.hashnode.dev/create-a-web-api-with-jwt-authentication-and-aspnet-core-identity

namespace WebApplicationJWT.Models
{
    public class Article
    {
        public Guid ArticleId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int Views { get; set; }
        public int UpVotes { get; set; }
    }
}