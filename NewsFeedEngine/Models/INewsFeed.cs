using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsFeedEngine.Models
{
    public interface INewsArticleFeed
    {
        string GetCountry(string country);
    }

}
