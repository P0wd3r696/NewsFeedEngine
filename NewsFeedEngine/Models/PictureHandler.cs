using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace NewsFeedEngine.Models
{
    public class PictureHandler
    {
        public bool _isValid;

        public string GetPictures(string rssLink, string parent, string child)
        {
            var xml = XDocument.Parse(rssLink);
            IEnumerable<NewsArticle> rssFeedData;
            if (!_isValid)
            {
                rssFeedData = xml.Descendants(parent)
                    .Select(x => new NewsArticle
                    {
                        Picture = (string) x.Element(child)?.Attribute("url")
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
                    .Select(x => new NewsArticle
                    {
                        Picture = (string) x.Element(child)?.Element("url")
                    });
                _isValid = false;
            }

            //if (string.IsNullOrEmpty(rssFeedData.FirstOrDefault()?.Picture))
            //{
            //    GetPictures(client, rssLink, "channel", "image");
            //}
            return rssFeedData.FirstOrDefault()?.Picture;
        }
    }
}