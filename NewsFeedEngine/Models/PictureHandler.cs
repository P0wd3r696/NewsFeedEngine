using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using HtmlAgilityPack;

namespace NewsFeedEngine.Models
{
    public class PictureHandler
    {
        private bool _isValid;

        public string GetPictures(string link, string parent, string child, NewsArticle newsArticle)
        {
            var xml = XDocument.Parse(link);
            IEnumerable<NewsArticle> rssFeedData;
            if (link.Contains("rss"))
            {
                if (!_isValid)
                {
                    rssFeedData = xml.Descendants(parent)
                        .Select(x => new NewsArticle
                        {
                            Picture = (string)x.Element(child)?.Attribute("url")
                        });
                    if (string.IsNullOrEmpty(rssFeedData.FirstOrDefault()?.Picture))
                    {
                        _isValid = true;
                        return GetPictures(link, "channel", "image", newsArticle);
                    }
                }
                else
                {
                    rssFeedData = xml.Descendants(parent)
                        .Select(x => new NewsArticle
                        {
                            Picture = (string)x.Element(child)?.Element("url")
                        });
                    _isValid = false;
                }
                return rssFeedData.FirstOrDefault()?.Picture;
            }
            else if (link.Contains("atom"))
            {
                HtmlDocument htmlDoc = new HtmlDocument(); htmlDoc.LoadHtml(newsArticle.Summary);
                if (htmlDoc.DocumentNode.SelectNodes("//img[@src]") != null)
                {
                    newsArticle.Picture = htmlDoc.DocumentNode.SelectNodes("//img[@src]").FirstOrDefault()?.Attributes[0].Value;

                }
                newsArticle.Summary = Regex.Replace(htmlDoc.DocumentNode.SelectNodes("//p")[0].InnerHtml, @"\t|\n|\r", "").Trim();
                return newsArticle.Picture;
            }

            return null;
        }
    }
}