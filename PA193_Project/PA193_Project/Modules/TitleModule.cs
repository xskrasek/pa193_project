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
            // TODO extracts the first line for now (testing purposes)
            List<string> pages = Enumerable.Range(0, 2).Select(n => document.GetPage(n)).ToList();
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

            title = Regex.Replace(title.Trim(), @"\r\n?|\n", " ");
            title = Regex.Replace(title, @"[ ]{2,}", " ");

            intermmediateResult.Title = title;
        }
    }
}
