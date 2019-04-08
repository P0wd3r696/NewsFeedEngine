using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace NewsFeedEngine.Models
{
    public class PictureHandler
    {
        private bool _isValid;

        public string GetPictures(string link, string parent, string child)
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
                        return GetPictures(link, "channel", "image");
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
                rssFeedData = xml.Descendants(parent)
                    .Select(x => new NewsArticle
                    {
                        Picture = (string)x.Element(child)?.Attribute("src")
                    });
                return rssFeedData.FirstOrDefault()?.Picture;
            }
            //if (string.IsNullOrEmpty(rssFeedData.FirstOrDefault()?.Picture))
            //{
            //    GetPictures(client, link, "channel", "image");
            //}
            //return rssFeedData.FirstOrDefault()?.Picture;
            return null;
        }
    }
}