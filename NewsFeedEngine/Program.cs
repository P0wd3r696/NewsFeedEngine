using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using NewsFeedEngine.Models;

namespace NewsFeedEngine
{
    public class Program
    {
        private static readonly NewsFeedEngine.Models.NewsDBEntities Context = new NewsDBEntities();

        static void Main(string[] args)
        {
            GetRssFeed();
        }

        public static bool IsRssFeed(string rssData)
        {
            if (rssData.Contains("<channel"))
                return true;
            else if (rssData.Contains("<entry>"))
                return false;

            return false;
        }
        public static void GetRssFeed()
        {
            RssHandler rssHandler = new RssHandler();
            AtomHandler atomHandler = new AtomHandler();
            foreach (var feed in Context.NewsFeeds.ToList())
            {
                var rssLink = feed.RssUrl;
                WebClient client = new WebClient();
                if (rssLink != null)
                {
                    var rssData = client.DownloadString(rssLink);
                    var isRssFeed = IsRssFeed(rssData);
                    if (isRssFeed)
                    {
                        rssHandler.GetRssFeed(rssData, feed.CategoryId);
                    }
                    else
                    {
                        atomHandler.GetRssFeed(rssData, feed.CategoryId);
                    }
                }
            }
        }


    }
}
