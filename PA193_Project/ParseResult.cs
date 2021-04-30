using System.Collections.Generic;

namespace PA193_Project.Entities
{
    class ParseResult
    {
        public string title { get; set; } = "";
        public Dictionary<string, HashSet<string>> versions { get; set; } = new Dictionary<string, HashSet<string>>();
        // I really don't want to use dynamic here. But the array really are dynamic with 2 strings and a number.
        public dynamic[][] table_of_contents { get; set; } = System.Array.Empty<dynamic[]>();
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
