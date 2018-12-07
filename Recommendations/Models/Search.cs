using NICE.Search.Common.Models;
using NICE.Search.Common.Urls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Recommendations.Models
{
    public class Search
    {
        public SearchUrl SearchUrl { get; set; }

        public string q { get; set; }

        public SearchResults Results { get ; set;}
    }
}