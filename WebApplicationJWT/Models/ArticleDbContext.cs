//  https://memorycrypt.hashnode.dev/create-a-web-api-with-jwt-authentication-and-aspnet-core-identity

using Microsoft.EntityFrameworkCore;

namespace WebApplicationJWT.Models
{
    public class ArticleDbContext : DbContext
    {
        public ArticleDbContext(DbContextOptions<ArticleDbContext> options) : base(options) { }
        public DbSet<Article> Articles { get; set; }
    }
}