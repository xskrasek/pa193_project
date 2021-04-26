using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using PA193_Project.Entities;

namespace PA193_Project.Modules
{
    class TitleModule : IModule
    {
        /// <summary>
        /// Applies various heuristics to extract the title
        /// <list type="number">
        /// <item>Fetch first 10 pages</item>
        /// <item>Determine if the document is a single or two pages</item>
        /// <item>Find lines enveloped by whitespace</item>
        /// </list>
        /// </summary>
        /// <param name="document">Document to extract from</param>
        /// <param name="intermmediateResult">Result to store the title in</param>
        public void Extract(Document document, ref ParseResult intermmediateResult)
        {
            List<string> pages = Enumerable.Range(0, 2).Select(n => document.GetPage(n)).ToList();

            string title = this.STLineHeuristic(pages[0]); // TODO iterate over pages instead of assuming 0
            if (title.Length == 0) title = this.BlankForHeuristic(pages[0]);
            if (title.Length == 0) title = this.BlankLineHeurustic(pages);

            title = Regex.Replace(title.Trim(), @"\r\n?|\n", " ");
            title = Regex.Replace(title, @"[ ]{2,}", " ");
            intermmediateResult.title = title;
        }


        private string BlankLineHeurustic(List<string> pages)
        {
            /*
             * Regex explanation:
             * (?:\r?\n|^) = Look for any potential blank lines before the main group
             * ((?:\r?\n = Match any line that may contain either whitespace,...
             *          |. = ...or any non-whitespace character, that is NOT followed by...
             *            (?!\.(\s|$)) = ...a literal dot at the end of the line (sentences)...
             *                         = ... or a single whitespace (sentences in paragraphs).
             *                        )+?) = There should be more lines like that
             * (?=\r?\n\r?\n|$) = And also there should be blank lines after the main group
             */
            Regex sectionsRegex = new Regex(@"(?:\r?\n|^)((?:\r?\n|.(?!\.(\s|$)))+?)(?=\r?\n\r?\n|$)",
                RegexOptions.Compiled);
            Regex colonFieldRegex = new Regex(@"\s*\S+:.*", RegexOptions.Compiled);
            Regex isoDateRegex = new Regex(@"\d{4}-\d{2}-\d{2}", RegexOptions.Compiled);

            string title = "";

            for (int pageIdx = 0; pageIdx < pages.Count; pageIdx++)
            {
                string page = pages[pageIdx];
                MatchCollection matches = sectionsRegex.Matches(page);
                foreach (Match match in matches)
                {
                    if (!colonFieldRegex.Match(match.Value).Success && !isoDateRegex.Match(match.Value).Success)
                    {
                        title = match.Value;
                        break;
                    }
                }
                if (title.Length > 0) { break; }
            }

            return title;
        }

        private string STLineHeuristic(string page)
        {
            // Security Target Lite line should be on the first page, so we can check only that
            List<string> lines = page.Split("\n").ToList();
            var stlines = lines.Where(l => l.Trim().ToLower().StartsWith("security target")).ToList();
            if (stlines.Count == 0) return "";
            string block = this.GetBlock(lines, lines.IndexOf(stlines[0]));
            List<string> blockLines = block.Split('\n').ToList();

            int stlineIndex = blockLines.Select(l => l.Trim().ToLower()).ToList().IndexOf("security target lite");
            // Remove the st line if it is the first line
            // This may not be necessary, because some jsons do include it
            if (stlineIndex == 0)
                block = String.Join('\n', blockLines.GetRange(1, blockLines.Count));

            // Some NXP documents have stline after the title
            if (blockLines[stlineIndex + 1].Trim().ToLower().StartsWith("rev"))
            {
                block = String.Join('\n', blockLines.GetRange(0, stlineIndex));
            }

            return block;
        }

        private string BlankForHeuristic(string page)
        {
            List<string> lines = page.Split('\n').Select(l => l.Trim()).ToList();
            int forIndex = lines.IndexOf("for");
            string block = this.GetBlock(lines, forIndex + 2);
            int fromIndex = block.IndexOf("from");
            if (fromIndex != -1)
                block = block.Substring(0, fromIndex);

            return block;
        }

        private string GetBlock(List<string> fragment, int startIndex)
        {
            int i = startIndex;

            while (fragment[i].Trim().Length > 0 && i < fragment.Count) i++;
            int end = i;

            i = startIndex;
            while (fragment[i].Trim().Length > 0 && i > 0) i--;
            int start = (fragment[i].Trim().Length > 0) ? 0 : i + 1;

            return String.Join('\n', fragment.GetRange(start, end));
        }
    }
}
