using NICE.Search.Common.Urls;
using NICE.Search.Providers;
using Recommendations.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Recommendations.Controllers
{
    public class SearchController : Controller
    {
        public ActionResult Index(SearchUrl searchUrl)
        {
            var provider = new SearchProvider(NICE.Search.Common.Enums.ApplicationEnvironment.RecommendationsLocal);

            var results = provider.Search(searchUrl);

            var viewModel = new Search { SearchUrl = searchUrl, Results = results };

            return View(viewModel);
        }
    }
}