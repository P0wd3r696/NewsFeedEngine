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
            try
            { rssData = EncodeText(rssData);
                var x = ChangeTextToHtml(rssData);
               
                XDocument doc = XDocument.Load(x);
                // Feed/Entry
                var entries = from item in doc.Root.Elements().Where(i => i.Name.LocalName == "entry")
                              select new NewsArticle
                              {
                                  //FeedType = FeedType.Atom,
                                  Summary = item.Elements().First(i => i.Name.LocalName == "content").Value,
                                  LinkUrl = item.Elements().First(i => i.Name.LocalName == "link").Attribute("href")?.Value,
                                  Picture = item.Elements().First(i => i.Name.LocalName == "link").Element("img")?.Value,
                                  //PublishDate = ParseDate(item.Elements().First(i => i.Name.LocalName == "published").Value),

                                  Title = item.Elements().First(i => i.Name.LocalName == "title").Value
                              };
                //return entries.ToList();

                SaveToDb(entries, rssData);
                Console.WriteLine("End!");
            }
            catch (Exception ex)
            {
                //return new List<NewsArticle>();
                Console.WriteLine("An error has occured");
            }
        }

        private string ChangeTextToHtml(string rssData)
        {
            var xml = WebUtility.HtmlDecode(rssData);
            return xml;
        }
    }
}