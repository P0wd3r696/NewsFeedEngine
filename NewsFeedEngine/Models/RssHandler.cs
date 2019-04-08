using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using NewsFeedEngine.Utilities;

namespace NewsFeedEngine.Models
{
    public class RssHandler
    {
        private readonly NewsDBEntities _context = new NewsDBEntities();
        private readonly PictureHandler _pictureHandler = new PictureHandler();
        public void SaveToDb(IEnumerable<NewsArticle> rssFeed, string rssData)
        {
            foreach (var rss in rssFeed)
                if (!_context.NewsArticles.Any(x => x.Title.Trim() == rss.Title.Trim()))
                {
                    var text = EncodeText(rss.Title);
                    rss.Summary = EncodeText(rss.Summary);
                    if (rss.Picture == null) rss.Picture = _pictureHandler.GetPictures(rssData, StaticData.RssItem, StaticData.RssEnclosure);
                    var splittedRssTitle = text.Split('|');
                    if (splittedRssTitle.Length == 1)
                    {
                        var splittedTitle = splittedRssTitle[0];
                        if (rss.Summary == string.Empty) continue;
                        try
                        {
                            if (_context.NewsArticles.Any(x=>x.Title != splittedTitle))
                            {
                                rss.ProviderId = _context.NewsProviders.FirstOrDefault(x => rss.LinkUrl.Contains(x.Name))?
                                    .ProviderId;
                                rss.Title = splittedRssTitle[0].Trim();
                                _context.NewsArticles.Add(rss);
                                _context.SaveChanges();
                            }
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
                            rss.ProviderId = _context.NewsProviders.FirstOrDefault(x => rss.LinkUrl.Contains(x.Name))?
                                .ProviderId;
                            rss.Title = splittedRssTitle[1].Trim();
                            _context.NewsArticles.Add(rss);
                            _context.SaveChanges();
                            Console.WriteLine($"Title: {rss.Title}");
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("An error has occured");
                        }
                    }
                }
        }


        public string EncodeText(string rssTitle)
        {
            var bytes = Encoding.Default.GetBytes(rssTitle);
            return Encoding.UTF8.GetString(bytes);
        }

        public void GetRssFeed(string rssData)
        {
            var xml = XDocument.Parse(rssData);

            var rssFeedData = xml.Descendants("item")
                .Select(x => new NewsArticle
                {
                    Title = (string)x.Element("title"),
                    LinkUrl = (string)x.Element("link"),
                    Summary = (string)x.Element("description"),
                    Picture = (string)x.Element("image")?.Element("url")
                });
            
            SaveToDb(rssFeedData, rssData);
            Console.WriteLine("End!");
        }
    }
}