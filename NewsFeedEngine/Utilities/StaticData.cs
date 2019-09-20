using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace NewsFeedEngine.Utilities
{
    public static class StaticData
    {
        public static string RssItem = "item";
        public static string RssItemImage = "image";
        public static string RssEnclosure = "enclosure";
        public static string AtomItem = "entry";
        public static string AtomItemImage = "content";

        public static string GetImageSource(string text)
        {
            string matchString = Regex.Match(text, "<img.+?src=[\"'](.+?)[\"'].*?>", RegexOptions.IgnoreCase).Groups[1].Value;
            //matchString = Regex.Replace(text, @"<img src\s*=", string.Empty);
            //matchString = Regex.Replace(text, @"\s*/>", string.Empty);
            return matchString;
        }
        public static string CleanDescription(string description)
        {
            string cleanHtml = Regex.Replace(description, @"<[^>]*>", string.Empty);
            return cleanHtml;
        }
        public static string CleanTitleForSEO(string title)
        {
            string replace1 = Regex.Replace(title, @"[£:;'‘’“$?â€™,.!@#%^&*()\/]", string.Empty);
            string replace2 = Regex.Replace(replace1, @"[_ ]", "-");
            return replace2.ToLower();
        }
        /// <summary>
        /// Determines a text file's encoding by analyzing its byte order mark (BOM).
        /// Defaults to ASCII when detection of the text file's endianness fails.
        /// </summary>
        /// <param name="filename">The text file to analyze.</param>
        /// <returns>The detected encoding.</returns>
        public static Encoding GetEncoding(byte[] bom)
        {
            // Read the BOM
            //var bom = new byte[4];
            //using (var file = new FileStream(filename, FileMode.Open, FileAccess.Read))
            //{
            //    file.Read(bom, 0, 4);
            //}

            // Analyze the BOM
            if (bom[0] == 0x2b && bom[1] == 0x2f && bom[2] == 0x76) return Encoding.UTF7;
            if (bom[0] == 0xef && bom[1] == 0xbb && bom[2] == 0xbf) return Encoding.UTF8;
            if (bom[0] == 0xff && bom[1] == 0xfe) return Encoding.Unicode; //UTF-16LE
            if (bom[0] == 0xfe && bom[1] == 0xff) return Encoding.BigEndianUnicode; //UTF-16BE
            if (bom[0] == 0 && bom[1] == 0 && bom[2] == 0xfe && bom[3] == 0xff) return Encoding.UTF32;
            return Encoding.Default;
        }
    }
}