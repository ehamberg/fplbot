using System.Net.Http;
using System.Threading.Tasks;
using Fpl.Search.Models;
using Fpl.Search.Searching;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FplBot.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _searchService;
        private readonly ILogger<SearchController> _logger;

        public SearchController(ISearchService searchService, ILogger<SearchController> logger)
        {
            _searchService = searchService;
            _logger = logger;
        }
        
        [HttpGet("entries/{query}")]
        public async Task<IActionResult> GetEntries(string query)
        {
            try
            {
                var metaData = new SearchMetaData
                {
                    Client = QueryClient.Web, Actor = Request?.HttpContext.Connection.RemoteIpAddress?.ToString()
                };

                var searchResult = await _searchService.SearchForEntry(query, 10, metaData);
                return Ok(new
                {
                    Hits = searchResult,
                });
            }
            catch (HttpRequestException e)
            {
                _logger.LogWarning(e.ToString());
            }
            return NotFound();
        }
    }
}