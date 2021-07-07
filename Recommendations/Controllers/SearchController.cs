using NICE.Search.Common.Enums;
using NICE.Search.Common.Urls;
using NICE.Search.HttpClient;
using Recommendations.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Recommendations.Controllers
{
    public class SearchController : Controller
    {
        public ActionResult Index(SearchUrl searchUrl)
        {
            /*TODO:Uncomment this to rollback to legacy NICE.Search Client
            var provider = new SearchProvider((ApplicationEnvironment)Enum.Parse(typeof(ApplicationEnvironment), ConfigurationManager.AppSettings["ElasticsearchEnvironment"]));

            var results = provider.Search(searchUrl); */

            var indexToQuery = "recommendations";
            var httpClientWrapper = new HttpClientWrapper();
            var searchHttpClient = new SearchHttpClient(ConfigurationManager.AppSettings["SearchApiUrl"], indexToQuery, httpClientWrapper);
            var results = searchHttpClient.Search(searchUrl);

            var viewModel = new Search { SearchUrl = searchUrl, Results = results };

            return View(viewModel);
        }
    }
}