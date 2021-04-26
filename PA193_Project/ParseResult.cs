using System.Collections.Generic;

namespace PA193_Project.Entities
{
    class ParseResult
    {
        public string title { get; set; } = "";
        public Dictionary<string, HashSet<string>> versions { get; set; } = new Dictionary<string, HashSet<string>>();
        public string[][] table_of_contents { get; set; } = System.Array.Empty<string[]>();
        public Revision[] revisions { get; set; } = System.Array.Empty<Revision>();
        public Dictionary<string, string> bibliography { get; set; } = new Dictionary<string, string>();

        public class Revision
        {
            string version;
            string date;
            string description;
        }
    }
}
