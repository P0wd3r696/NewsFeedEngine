using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using HtmlAgilityPack;
using NewsFeedEngine.Utilities;

namespace NewsFeedEngine.Models
{
    public class AtomHandler
    {
        private readonly NewsDBEntities _context = new NewsDBEntities();
        private readonly PictureHandler _pictureHandler = new PictureHandler();

        public void SaveToDbAtom(IEnumerable<NewsArticle> rssFeed, string rssData, int? categoryId)
        {
            foreach (var rss in rssFeed)
            {
                if (!_context.NewsArticles.Any(x => x.Title.Trim() == rss.Title.Trim()))
                {
                    var text = EncodeText(rss.Title);
                    rss.Summary = EncodeText(rss.Summary);
                    if (rss.Picture == null)
                        rss.Picture =
                            _pictureHandler.GetPictures(rssData, StaticData.AtomItem, StaticData.AtomItemImage, rss);
                    var splittedRssTitle = text.Split('|');
                    if (splittedRssTitle.Length == 1)
                    {
                        if (rss.Summary == string.Empty) continue;
                        try
                        {
                            rss.CategoryId = categoryId;
                            rss.ProviderId = _context.NewsProviders.FirstOrDefault(x => rss.LinkUrl.Contains(x.Name.Replace(" ","")))?
                                .ProviderId;
                            rss.Summary = rss.Summary.Trim();
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
                            rss.CategoryId = categoryId;
                            rss.ProviderId = _context.NewsProviders.FirstOrDefault(x => rss.LinkUrl.Contains(x.Name.Replace(" ", "")))?
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
        }

        public string EncodeText(string rssTitle)
        {
            var bytes = Encoding.Default.GetBytes(rssTitle);
            return Encoding.UTF8.GetString(bytes);
        }

        public void GetRssFeed(string rssData,int? categoryId)
        {
            try
            {
                Regex r = new Regex(@"\s=\s");
                var y = r.Replace(rssData, "=");
                XDocument doc = XDocument.Parse(y);
                var entries = from item in doc.Root.Elements().Where(i => i.Name.LocalName == "entry")
                              select new NewsArticle
                              {
                                  Summary = item.Elements().First(i => i.Name.LocalName == "content").Value,
                                  LinkUrl = item.Elements().First(i => i.Name.LocalName == "link").Attribute("href")?.Value,
                                  Picture = item.Elements().First(i => i.Name.LocalName == "content").Attribute("src")?.Value,
                                  Title = item.Elements().First(i => i.Name.LocalName == "title").Value,
                                  pubDate = Convert.ToDateTime(item.Elements().First(i => i.Name.LocalName == "updated").Value)
                              };
                SaveToDbAtom(entries, rssData,categoryId);
                Console.WriteLine("End!");
            }
            catch (XmlException ex)
            {

                //return new List<NewsArticle>();
                Console.WriteLine("An error has occured" + ex.Message);
            }
        }

        private string ChangeTextToHtml(string rssData)
        {
            var xml = WebUtility.HtmlDecode(rssData);
            return xml;
        }
    }
}