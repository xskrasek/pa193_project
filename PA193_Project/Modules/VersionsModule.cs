using System.Collections.Generic;
using System.Text.RegularExpressions;
using PA193_Project.Entities;

namespace PA193_Project.Modules
{
    internal class VersionsModule : IModule
    {
        public void Extract(Document document, ref ParseResult intermmediateResult)
        {
            var result = new Dictionary<string, HashSet<string>>();
            var toMatch = new List<string>();
            var versions = new List<string> {"eal", "global_platfom", "java_card", "sha", "rsa", "ecc", "des"};
            var eal = @"EAL(\d|\s\d)(\+)?";
            var gp = @"Global(\s)?Platform \d(\.\d)*";
            var jc = @"Java Card \d(\.\d)*";
            var sha = @"SHA(-|\s)?\d{1,3}";
            var rsa = @"RSA((\s|-|_)?\d{3,4}(\/\d{3,4})?|-CRT|SignaturePKCS1|SSA-PSS)";
            var ecc = @"ECC(\s|-)?\d{1,4}?";
            var des = @"(single|3|Triple|T)?(\s|-)?DES\d?";

            toMatch.AddRange(new List<string> {eal, gp, jc, sha, rsa, ecc, des});
            var i = 0;
            foreach (var pattern in toMatch)
            {
                var matches = Regex.Matches(document.FullText, pattern);
                //For triple-des
                if (i == 6) matches = Regex.Matches(document.FullText, pattern, RegexOptions.IgnoreCase);
                var matchesString = new HashSet<string>();
                foreach (Match match in matches)
                {
                    if (match.Value.Trim().ToUpper() != "DES") matchesString.Add(match.Value.Trim());
                }
                if (matchesString.Count != 0) result.Add(versions[i], matchesString);
                i++;
            }

            intermmediateResult.versions = result;
        }
    }
}