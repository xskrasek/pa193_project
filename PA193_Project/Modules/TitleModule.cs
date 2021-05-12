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
            Dictionary<String, int> candidates = new Dictionary<string, int>();

            for (int pageIdx = 0; pageIdx < pages.Count; pageIdx++)
            {
                string page = pages[pageIdx];
                MatchCollection matches = sectionsRegex.Matches(page);
                foreach (Match match in matches)
                {
                    // possibly add some manufacturer detection
                    if (!colonFieldRegex.Match(match.Value).Success && !isoDateRegex.Match(match.Value).Success)
                    {
                        if (match.Value.Trim().Length > 0 && !candidates.ContainsKey(match.Value))
                            candidates.Add(match.Value, this.calculateScore(match.Value));
                    }
                }
                if (title.Length > 0) { break; }
            }

            title = candidates.Aggregate((x, y) => x.Value > y.Value ? x : y).Key;
            //candidates.Select(i => $"{i.Key.Trim().Substring(0, Math.Min(40, i.Key.Trim().Length))}: {i.Value}").ToList().ForEach(Console.Error.WriteLine);

            return title;
        }

        private int calculateScore(string value)
        {
            // Keywords are either manufacturer names, well-known models or generic keywords related to certification targets
            String[] keywords = { "nxp", "infineon", "jcop", "ifx_cci", "idemia", "optigatm",
                                  "crypto", "librar", "module", "smart", "card", "controller" };
            String[] negativeKeywords = { "eal", "common", "criteria", "certificate" };
            int score = 0;
            foreach (string word in keywords)
            {
                // 10 pts for keyword match
                if (value.ToLower().Contains(word)) score += 10;
            }

            foreach (string word in negativeKeywords)
            {
                // deduct 7 points for negative keyword
                if (value.ToLower().Contains(word)) score -= 7;
            }

            // 5pts for model number match (awarded only once)
            MatchCollection modelMatches = new Regex(@"([A-Z0-9_]){3,}", RegexOptions.Compiled).Matches(value);
            foreach(Match match in modelMatches)
            {
                if (!keywords.Contains(match.Value.ToLower()))
                {
                    score += 5;
                    break;
                }
            }

            // 3+3pts for the version number
            Match versionMatch = new Regex(@"\sv?[0-9\.]+(\s|$)", RegexOptions.Compiled | RegexOptions.IgnoreCase).Match(value.ToLower());
            if (versionMatch.Success) score += 3;
            // +3 pts if it really is a version number, not just a number
            if (versionMatch.Value.StartsWith('v') || versionMatch.Value.Contains('.')) score += 3;

            return score;
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
            if (stlineIndex == 0 && block.Length > 1)
                block = String.Join('\n', blockLines.Skip(1));

            // Some NXP documents have stline after the title
            if (stlineIndex + 1 < blockLines.Count && blockLines[stlineIndex + 1].Trim().ToLower().StartsWith("rev"))
            {
                block = String.Join('\n', blockLines.GetRange(0, stlineIndex));
            }

            // Some NSCIB-CC... files have title preceeding the stline
            if (block.ToLower().Contains("nscib"))
            {
                // we need a new block with contents before the stline block
                // therefore we need to look for the string in all of the lines
                int newIndex = lines.Select(l => l.Trim().ToLower()).ToList().IndexOf("security target lite");
                block = this.GetBlock(lines, newIndex - 2);
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
            while (i < fragment.Count && fragment[i].Trim().Length > 0) i++;
            int end = i;

            i = startIndex;
            while (i > 0 && fragment[i].Trim().Length > 0) i--;
            int start = (i >= fragment.Count || fragment[i].Trim().Length > 0) ? 0 : i + 1;

            return String.Join('\n', fragment.GetRange(Math.Max(start, 0), Math.Max(end - start, 0)));
        }
    }
}
