using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PA193_Project.Entities;

namespace PA193_Project.Modules
{
    class TOCModule : IModule
    {
        public void Extract(Document document, ref ParseResult intermmediateResult)
        {
            Match tocMatch = this.FindTOCStart(document);
            List<string> lines = document.FullText.Substring(tocMatch.Index).Split("\n").ToList();
            List<string> tocLines = new List<string>();

            int limit = 10, idx = 0;
            while (limit > 0 && idx < lines.Count)
            {
                int occurences = 0;

                foreach (char c in lines[idx].Trim())
                    if (c == '.' || c == ' ') occurences++;

                if (occurences > 5) // these numbers are kinda arbitrary
                {
                    tocLines.Add(lines[idx]);
                    limit = 10;
                }

                idx++;
                limit--;
            }

            // This one does not seem very complex, but it took me a damn long time to come up with
            // The main issue was, that I want to match a dot character in the ToC line, but not the
            // repeated sequence of dots. I've tried messing around with negative lookahead, but to no avail.
            // So I'll match the whole thing (line guiding dots included) and then remove the dots manually.
            // Explanation:
            // \s* = whitespace before, the lines should be trimmed, but you never know
            //   ((?:\d\.?)+) = the first group with the section number, that may end with a dot
            //               \s* = more whitespace
            //                  ([^\%]+) = the % character serves as an anchor, that prevents the group from capturing
            //                             the number at the end. It is put in place instead of the repeating dots.
            //                          \s* = more whitespace
            //                             (\d+) = the page number
            Regex tocLineParseRegex = new Regex(@"^%?\s*((?:\d\.?)*)%?\s*([^\%]+)%\s*(\d+)$",
                RegexOptions.Compiled | RegexOptions.Multiline);
            var results = new List<dynamic[]>();
            foreach(string line in tocLines)
            {
                // The % character serves as an anchor that prevents the ([^\%]+) from greedily matching the number at the end
                string modifiedLine = Regex.Replace(line, @"[\. ]{2,}", "%");
                Match match = tocLineParseRegex.Match(modifiedLine);
                if (!match.Success) continue;
                // I know, I know. Boo hoo dynamic bad. But the output array has 2 strings and an integer
                // so I have to throw the type system out of the window :/
                dynamic[] parsedLine = new dynamic[3];
                parsedLine[0] = match.Groups[1].Value.Trim();
                parsedLine[1] = match.Groups[2].Value.Trim();

                try
                {
                    parsedLine[2] = int.Parse(match.Groups[3].Value.Trim());
                    results.Add(parsedLine);
                }
                catch (System.OverflowException)
                {
                }
            }

            intermmediateResult.table_of_contents = results.ToArray();
        }

        private Match FindTOCStart(Document document)
        {
            string haystack = document.FullText;
            
            // TODO add explanation here, though this one isn't that difficult to understand
            Match match = new Regex(@"\s*([0-9]+\.\s*)?(table of )?contents:?", RegexOptions.IgnoreCase).Match(haystack);
            return match;
        }
    }
}
