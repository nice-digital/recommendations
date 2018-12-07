using NICE.Search.Common.Enums;
using NICE.Search.Common.Urls;
using NICE.Search.Providers;
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
            var provider = new SearchProvider((ApplicationEnvironment)Enum.Parse(typeof(ApplicationEnvironment), ConfigurationManager.AppSettings["ElasticsearchEnvironment"]));

            var results = provider.Search(searchUrl);

            var viewModel = new Search { SearchUrl = searchUrl, Results = results };

            return View(viewModel);
        }
    }
}