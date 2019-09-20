using NewsFeedEngine.Models;
using System;
using System.Linq;
using System.Net;

namespace NewsFeedEngine
{
    public class Program
    {
        private static readonly NewsDBEntities Context = new NewsDBEntities();

        private static void Main(string[] args)
        {
            GetRssFeed();
            //Console.ReadLine();
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
            foreach (var feed in Context.NewsFeeds.Where(x => x.Active == true).ToList())
            {
                try
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
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            //Console.ReadLine();
        }
    }
}