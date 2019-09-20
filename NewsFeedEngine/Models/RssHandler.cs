using System;
using System.Collections.Generic;
using System.Globalization;
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

        public void SaveToDb(IEnumerable<NewsArticle> rssFeed, string rssData, int? categoryId)
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

        public bool IsHtmlDecoded(string text)
        {
            bool result;
            var message = WebUtility.HtmlDecode(text);
            if (message != text)
                result = false;
            else
                result = true;

            return result;
        }

        public string DecodeToWIndows1252(string text)
        {
            var windows1252 = Encoding.GetEncoding("Windows-1252");
            var bytes = Encoding.Default.GetBytes(text);
            var result = Encoding.Default.GetString(bytes);
            //if (!IsHtmlDecoded(result))
            //{
            //    return Encoding.UTF8.GetString(bytes);
            //}
            return WebUtility.HtmlDecode(result);
        }

        public string EncodeToWIndows1252(string text)
        {
            Encoding windows1252 = Encoding.GetEncoding("Windows-1252");
            var bytes = windows1252.GetBytes(text);
            var result = Encoding.Default.GetString(bytes);

            return result;
        }

        private string ChangeTextToHtml(string rssData)
        {
            var xml = WebUtility.HtmlDecode(rssData);
            return xml;
        }

        public string DecodeText(string rssTitle)
        {
            var bytes = Encoding.Default.GetBytes(rssTitle);
            return Encoding.UTF8.GetString(bytes);
        }

        public string EncodeText(string rssTitle)
        {
            return WebUtility.HtmlEncode(rssTitle);
        }

        public void GetRssFeed(string rssData, int? categoryId)
        {
            var date = new DateTime();
            var xml = XDocument.Parse(rssData);
            try
            {
                var rssFeedDataList = xml.Descendants("channel");
                var rssFeedData = xml.Descendants("item")
                    .Select(x => new NewsArticle
                    {
                        Title = (string)x.Element("title"),
                        SEOURL = StaticData.CleanTitleForSEO((string)x.Element("title")),
                        Url = (string)x.Element("link"),
                        Summary = StaticData.CleanDescription((string)x.Element("description")),
                        //Picture = (string)x.Element("image")?.Element("url") == null?
                        //    "https://www.google.com/url?sa=i&rct=j&q=&esrc=s&source=images&cd=&ved=2ahUKEwiLuZTK_8_iAhUHQhoKHSBKCSUQjRx6BAgBEAQ&url=https%3A%2F%2Fwww.iconfinder.com%2Ficons%2F341106%2Frss_icon&psig=AOvVaw12ydppBMhEOH011OSC8R-1&ust=1559743515365402"
                        //    : (string)x.Element("image")?.Element("url"),
                        Picture = (string)x.Element("enclosure")?.Attribute("url") == null ? (string)x.Element("image")?.Element("url") : (string)x.Element("enclosure")?.Attribute("url"),
                        //CreatedDate = (string)x.Element("pubDate")
                        CreatedDate = !DateTime.TryParse((string)x.Element("pubDate"), CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out date)
                            ? DateTime.Now
                            : date
                    });
                SaveToDb(rssFeedData, rssData, categoryId);
                Console.WriteLine("End!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}