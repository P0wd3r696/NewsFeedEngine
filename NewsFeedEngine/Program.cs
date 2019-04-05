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

        public static void GetRssFeed()
        {

            foreach (var feed in Context.NewsFeeds.ToList())
            {
                var rssLink = feed.RssUrl;
                WebClient client = new WebClient();
                if (rssLink != null)
                {
                    GetRssFeed(rssLink, client);
                }
            }
        }

        private static void SaveToDb(IEnumerable<NewsArticle> rssFeed, WebClient client, string rssLink)
        {
            var rssData = client.DownloadString(rssLink);
            foreach (var rss in rssFeed)
            {
                if (!Context.NewsArticles.Any(x => x.Title.Trim() == rss.Title.Trim()))
                {
                    var text = EncodeText(rss.Title);
                    rss.Summary = EncodeText(rss.Summary);
                    if (rss.Picture == null)
                    {
                        rss.Picture = GetPictures(rssData, "item", "enclosure");
                    }
                    var splittedRssTitle = text.Split('|');
                    if (splittedRssTitle.Length == 1)
                    {
                        if (rss.Summary == string.Empty) continue;
                        try
                        {
                            rss.Title = splittedRssTitle[0].Trim();
                            Context.NewsArticles.Add(rss);
                            Context.SaveChanges();
                            Console.WriteLine($"Title: {rss.Title}");

                        }
                        catch (Exception e)
                        {
                            Console.Write("An error has occured.");
                        }
                    }
                    else
                    {
                        try
                        {
                            rss.Title = splittedRssTitle[1].Trim();
                            Context.NewsArticles.Add(rss);
                            Context.SaveChanges();
                            Console.WriteLine($"Title: {rss.Title}");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("An error has occured");
                        }
                    }
                }
            }
        }

        public static bool _isValid = false;
        private static string GetPictures(string rssLink, string parent, string child)
        {

            //var rssData = client.DownloadString(rssLink);

            XDocument xml = XDocument.Parse(rssLink);
            IEnumerable<NewsArticle> rssFeedData;
            if (!_isValid)
            {
                rssFeedData = xml.Descendants(parent)
                    .Select(x => new NewsArticle()
                    {
                        Picture = (string)x.Element(child)?.Attribute("url")
                    });
                if (string.IsNullOrEmpty(rssFeedData.FirstOrDefault()?.Picture))
                {
                    _isValid = true;
                    return GetPictures(rssLink, "channel", "image");

                }
            }
            else
            {
                rssFeedData = xml.Descendants(parent)
                    .Select(x => new NewsArticle()
                    {
                        Picture = (string)x.Element(child)?.Element("url")
                    });
                _isValid = false;
            }
            //if (string.IsNullOrEmpty(rssFeedData.FirstOrDefault()?.Picture))
            //{
            //    GetPictures(client, rssLink, "channel", "image");
            //}
            return rssFeedData.FirstOrDefault()?.Picture;
        }


        private static string EncodeText(string rssTitle)
        {
            var bytes = Encoding.Default.GetBytes(rssTitle);
            return Encoding.UTF8.GetString(bytes);

        }

        private static void GetRssFeed(string rssLink, WebClient client)
        {
            var rssData = client.DownloadString(rssLink);

            XDocument xml = XDocument.Parse(rssData);
            IEnumerable<NewsArticle> rssFeedData;
            rssFeedData = xml.Descendants("item")
                .Select(x => new NewsArticle()
                {
                    Title = (string) x.Element("title"),
                    LinkUrl = (string) x.Element("link"),
                    Summary = (string) x.Element("description"),
                    Picture = (string) x.Element("image")?.Element("url"),
                    ProviderId = 2
                });

            var rssFeed = rssFeedData;
            SaveToDb(rssFeed, client, rssLink);
            Console.WriteLine("End!");
        }
    }
}
