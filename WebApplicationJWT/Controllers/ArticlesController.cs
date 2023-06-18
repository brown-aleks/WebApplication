using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using WebApplicationJWT.Models;

namespace WebApplicationJWT.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class ArticlesController : Controller
    {
        private readonly ILogger<ArticlesController> _logger;
        private readonly ArticleDbContext _articleDbContext;

        public ArticlesController(ILogger<ArticlesController> logger, ArticleDbContext articleDbContext)
        {
            _logger = logger;
            _articleDbContext = articleDbContext;
        }

        [HttpGet]
        public ActionResult<IEnumerable<Article>> GetArticles()
        {
            return Ok(_articleDbContext.Articles.ToList());
        }


        [HttpGet("{id}")]
        public ActionResult<Article> GetArticles(string id)
        {
            var guid = new Guid(id);
            var article = _articleDbContext.Articles.FirstOrDefault(a => a.ArticleId.Equals(guid));
            if (article == null)
            {
                return NotFound();
            }

            return Ok(article);
        }

        [HttpPost]
        public ActionResult<Article> InsertArticle(Article article)
        {
            article.ArticleId = Guid.NewGuid();
            _articleDbContext.Articles.Add(article);
            _articleDbContext.SaveChanges();
            return CreatedAtAction(nameof(GetArticles), new { id = article.ArticleId }, article);
        }

        [HttpPut("{id}")]
        public ActionResult<Article> UpdateArticle(string id, Article article)
        {
            if (id != article.ArticleId.ToString())
            {
                return BadRequest();
            }

            var guid = new Guid(id);
            var articleToUpdate = _articleDbContext.Articles.FirstOrDefault(a => a.ArticleId.Equals(guid));

            if (articleToUpdate == null)
            {
                return NotFound();
            }

            articleToUpdate.Author = article.Author;
            articleToUpdate.Content = article.Content;
            articleToUpdate.Title = article.Title;
            articleToUpdate.UpVotes = article.UpVotes;
            articleToUpdate.Views = article.Views;

            _articleDbContext.Articles.Update(articleToUpdate);
            _articleDbContext.SaveChanges();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public ActionResult DeleteArticle(string id)
        {
            var guid = new Guid(id);
            var articleToDelete = _articleDbContext.Articles.FirstOrDefault(a => a.ArticleId.Equals(guid));

            if (articleToDelete == null)
            {
                return NotFound();
            }

            _articleDbContext.Articles.Remove(articleToDelete);
            _articleDbContext.SaveChanges();

            return NoContent();
        }

    }
}
