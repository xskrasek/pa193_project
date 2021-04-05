using System.Collections.Generic;

namespace PA193_Project.Entities
{
    class ParseResult
    {
        public string Title { get; set; }
        public Dictionary<string, List<string>> Versions { get; set; }
        public string[][] TableOfContents { get; set; }
        public Revision[] Revisions { get; set; }
        public Dictionary<string, string> Bibliography { get; set; }

        public class Revision
        {
            string version;
            string date;
            string description;
        }
    }
}
