using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using NewsFeedEngine.Utilities;

namespace NewsFeedEngine.Models
{
    public class AtomHandler
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
                    if (rss.Picture == null) rss.Picture = _pictureHandler.GetPictures(rssData, StaticData.AtomItem, StaticData.AtomItemImage);
                    var splittedRssTitle = text.Split('|');
                    if (splittedRssTitle.Length == 1)
                    {
                        if (rss.Summary == string.Empty) continue;
                        try
                        {
                            rss.Title = splittedRssTitle[0].Trim();
                            _context.NewsArticles.Add(rss);
                            _context.SaveChanges();
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

            var rssFeedData = xml.Descendants("entry")
                .Select(x => new NewsArticle
                {
                    Title = (string)x.Element("title"),
                    LinkUrl = (string)x.Element("link"),
                    Summary = (string)x.Element("content"),
                    Picture = (string)x.Element("image")?.Element("url"),
                    ProviderId = 2
                });

            SaveToDb(rssFeedData, rssData);
            Console.WriteLine("End!");
        }
    }
}