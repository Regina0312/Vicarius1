using Microsoft.AspNetCore.Mvc;
using Nest;
using VicariusAssignment.Models;

namespace VicariusAssignment.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ElasticController : Controller
    {
        private readonly IElasticClient _elasticClient;

        public ElasticController(IElasticClient elasticClient)
        {
            _elasticClient= elasticClient;
        }

        [HttpPost]
        [Route("/index")]
        public ActionResult CreateIndex(string indexName)
        {
            try
            { 
               var index = _elasticClient.Indices.Create(indexName, index => index.Map<Document>(x => x.AutoMap())); 
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }

        [HttpPost]
        [Route("/document")]
        public async Task<IActionResult> Create(Document document)
        {
            try
            {
                var doc = new Document()
                {
                    Id = 1,
                    Name = document.Name
                };
                await _elasticClient.IndexDocumentAsync(doc);
                document = new Document();
            }
            catch (Exception ex) 
            {
            }
            return View(document);
        }

        [HttpGet]
        [Route("document")]
        public IActionResult GetDocument(string key)
        {
            var articleList = new List<Document>();
            if (!string.IsNullOrEmpty(key))
            {
                articleList = GetSearch(key).ToList();
            }
            return View(articleList.AsEnumerable());
        }
        private IList<Document> GetSearch(string key)
        {
            var result = _elasticClient.SearchAsync<Document>(x => x.Query(q => q.QueryString(d => d.Query('*' + key + '*'))).Size(5000));
            var finalResult = result;
            var finalContent = finalResult.Result.Documents.ToList();
            return finalContent;
        }
    }
}
