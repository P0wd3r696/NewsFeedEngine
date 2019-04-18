using HtmlAgilityPack;
using NewsFeedEngine.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace NewsFeedEngine.Models
{
    public class RssHandler
    {
        private readonly NewsDBEntities _context = new NewsDBEntities();
        private readonly PictureHandler _pictureHandler = new PictureHandler();

        public void SaveToDb(IEnumerable<NewsArticle> rssFeed, string rssData, int? categoryId)
        {
            foreach (var rss in rssFeed)
                if (!_context.NewsArticles.Any(x => x.Title.Trim() == rss.Title.Trim()))
                {
                    var text = EncodeText(rss.Title);
                    rss.Summary = EncodeText(rss.Summary);
                    if (rss.Picture == null) rss.Picture = _pictureHandler.GetPictures(rssData, StaticData.RssItem, StaticData.RssEnclosure, rss);
                    var splittedRssTitle = text.Split('|');
                    if (splittedRssTitle.Length == 1)
                    {
                        var splittedTitle = splittedRssTitle[0];
                        if (rss.Summary == string.Empty) continue;
                        try
                        {
                            if (rss.Summary.Contains("<img"))
                            {
                                var doc = new HtmlDocument();
                                doc.LoadHtml(rss.Summary);
                                rss.Summary = EncodeText(doc.DocumentNode.SelectNodes("//p").FirstOrDefault()?.InnerText);
                                //rss.Picture = EncodeText(doc.DocumentNode.ChildNodes["p"]?.ChildNodes["a"].Attributes["src"].Value);
                                //rss.Picture = EncodeText(doc.DocumentNode.SelectNodes("//img[@src]").FirstOrDefault()
                                //    ?.Attributes[0].Value);
                            }
                            else if (rss.Summary.Contains("<p>") || rss.Summary.Contains("<a href"))
                            {
                                var doc = new HtmlDocument();
                                doc.LoadHtml(rss.Summary);
                                rss.Summary = EncodeText(doc.DocumentNode.SelectNodes("//p").FirstOrDefault()?.InnerText);
                                //rss.Picture = doc.DocumentNode.ChildNodes["p"].ChildNodes["a"].Attributes["src"].Value;
                                //rss.Picture = EncodeText(doc.DocumentNode.SelectNodes("//img[@src]").FirstOrDefault()
                                //?.Attributes["src"].Value);
                            }

                            if (_context.NewsArticles.Any(x => !x.Title.Contains(splittedTitle)))
                            {
                                //rss.Picture =null;
                                rss.ProviderId = _context.NewsProviders.FirstOrDefault(x => rss.Url.Contains(x.Name.Replace(" ", "")))?
                                    .ProviderId;
                                rss.CategoryId = categoryId;
                                rss.Title = EncodeText(splittedRssTitle[0].Trim());
                                //rss.pubDate = rss.pubDate;
                                _context.NewsArticles.Add(rss);
                                _context.SaveChanges();
                            }
                            Console.WriteLine($"Title: {rss.Title}");
                        }
                        catch (Exception e)
                        {
                            Console.Write(e.Message + " An error has occured.");
                        }
                    }
                    else
                    {
                        try
                        {
                            rss.ProviderId = _context.NewsProviders.FirstOrDefault(x => rss.Url.Contains(x.Name.Replace(" ", "")))?
                                .ProviderId;
                            rss.CategoryId = categoryId;
                            rss.Title = EncodeText(splittedRssTitle[1].Trim());
                            _context.NewsArticles.Add(rss);
                            _context.SaveChanges();
                            Console.WriteLine($"Title: {rss.Title}");
                        }
                        catch (Exception e)
                        {
                            Console.Write(e.Message + " An error has occured.");
                        }
                    }
                }
        }

        private string ChangeTextToHtml(string rssData)
        {
            var xml = WebUtility.HtmlDecode(rssData);
            return xml;
        }

        public string EncodeText(string rssTitle)
        {
            var bytes = Encoding.Default.GetBytes(rssTitle);
            return Encoding.UTF8.GetString(bytes);
        }

        public void GetRssFeed(string rssData, int? categoryId)
        {
            var xml = XDocument.Parse(rssData);

            var rssFeedData = xml.Descendants("item")
                .Select(x => new NewsArticle
                {
                    Title = (string)x.Element("title"),
                    Url = (string)x.Element("link"),
                    Summary = (string)x.Element("description"),
                    Picture = (string)x.Element("image")?.Element("url"),
                    CreatedDate = (DateTime)x.Element("pubDate")
                });

            SaveToDb(rssFeedData, rssData, categoryId);
            Console.WriteLine("End!");
        }
    }
}