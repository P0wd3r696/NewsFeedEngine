using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
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
                var splittedText = rss
                    .Title
                    .Split('|');
                //encode the title and summary
                var text = EncodeText(rss.Title);
                var summary = EncodeText(rss.Summary);
                if (rss.Title != text)
                {
                    if (text.Contains("â") || text.Contains("&#39;") || text.Contains("&#226;€™;"))
                    {
                        rss.Title = DecodeText(WebUtility.HtmlDecode(text));
                        if (summary.Contains("â") || summary.Contains("&#39;") || summary.Contains("&#226"))
                            rss.Summary = DecodeText(WebUtility.HtmlDecode(summary)).Trim();
                    }
                    else
                    {
                        rss.Title = DecodeText(WebUtility.HtmlDecode(text));
                        if (!summary.Contains("â") || !summary.Contains("&#39;") || !summary.Contains("&#226"))
                            rss.Summary = DecodeText(WebUtility.HtmlDecode(summary));
                    }
                }
                else
                {
                    if (text.Contains("â") || text.Contains("&#39;") || text.Contains("&#226;€™;"))
                    {
                        rss.Title = DecodeText(WebUtility.HtmlDecode(text));
                        if (summary.Contains("â") || summary.Contains("&#39;") || summary.Contains("&#226"))
                            rss.Summary = DecodeText(WebUtility.HtmlDecode(summary)).Trim();
                    }
                    else
                    {
                        rss.Title = DecodeText(WebUtility.HtmlDecode(text));

                        rss.Summary = DecodeText(WebUtility.HtmlDecode(summary)).Trim();
                    }
                }

                //if (rss.Picture == null)
                //    rss.Picture =
                //        _pictureHandler.GetPictures(rssData, StaticData.RssItem, StaticData.RssEnclosure, rss);
                var splittedRssTitle = rss.Title.Split('|');
                if (splittedRssTitle.Length == 1)
                {
                    var splittedTitle = splittedRssTitle[0].Trim();
                    if (!_context.NewsArticles.Any(x =>
                        x.Title.Trim() == rss.Title.Trim() || x.Title.Trim() == splittedTitle.Trim()))
                    {
                        if (rss.Summary == string.Empty) continue;
                        try
                        {
                            if (!_context.NewsArticles.Any(x => x.Title.Contains(splittedTitle)))
                            {
                                rss.ProviderId = _context.NewsProviders
                                    .FirstOrDefault(x => rss.Url.Contains(x.Name.Replace(" ", "")))?
                                    .ProviderId;
                                rss.CategoryId = categoryId;
                                rss.Title = splittedTitle.Trim();
                                rss.Summary = rss.Summary.Trim();
                                _context.NewsArticles.Add(rss);
                                _context.SaveChanges();
                                Console.WriteLine($"Title: {rss.Title}");
                            }
                        }
                        catch (Exception e)
                        {
                            Console.Write(e.Message + " An error has occured.");
                        }
                    }
                }
                else
                {
                    var splittedTitle = splittedRssTitle[1];
                    if (!_context.NewsArticles.Any(x => x.Title.Trim() == rss.Title.Trim() || x.Title.Trim() == splittedTitle.Trim()))
                    {
                        try
                        {
                            rss.ProviderId = _context.NewsProviders
                                .FirstOrDefault(x => rss.Url.Contains(x.Name.Replace(" ", "")))?
                                .ProviderId;
                            rss.CategoryId = categoryId;
                            rss.Title = splittedTitle.Trim();
                            rss.Summary = summary.Trim();
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
        }

        public string DecodeText(string rssTitle)
        {
            var bytes = Encoding.Default.GetBytes(rssTitle);
            return Encoding.UTF8.GetString(bytes);
        }

        public void GetRssFeed(string rssData, int? categoryId)
        {
            try
            {
                var r = new Regex(@"\s=\s");
                var y = r.Replace(rssData, "=");
                var doc = XDocument.Parse(y);
                var entries = from item in doc.Root.Elements().Where(i => i.Name.LocalName == "entry")
                              select new NewsArticle
                              {
                                  SEOURL = StaticData.CleanTitleForSEO(item.Elements().First(i => i.Name.LocalName == "title").Value),
                                  Summary = StaticData.CleanDescription(item.Elements().First(i => i.Name.LocalName == "content")
                                      .Value),
                                  Url = item.Elements().First(i => i.Name.LocalName == "link").Attribute("href")?.Value,
                                  Picture = StaticData.GetImageSource(item.Elements().First(i => i.Name.LocalName == "content").Value)
                                      // ? item.Elements().First(i => i.Name.LocalName == "content").Value)
                                      ,
                                  Title = item.Elements().First(i => i.Name.LocalName == "title").Value,
                                  CreatedDate =
                                      Convert.ToDateTime(item.Elements().First(i => i.Name.LocalName == "updated").Value)
                              };
                SaveToDbAtom(entries, rssData, categoryId);
                Console.WriteLine("End!");
            }
            catch (XmlException ex)
            {
                Console.WriteLine("An error has occured" + ex.Message);
            }
        }
        public string EncodeText(string rssTitle)
        {
            return WebUtility.HtmlEncode(rssTitle);
        }
        private string ChangeTextToHtml(string rssData)
        {
            var xml = WebUtility.HtmlDecode(rssData);
            return xml;
        }
    }
}