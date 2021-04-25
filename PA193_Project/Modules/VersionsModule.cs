using PA193_Project.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PA193_Project.Modules
{
    class VersionsModule : IModule
    {
        //TODO: Divade into grups by prefix and suffix and both. Will allow better preccision probably
        public void Extract(Document document, ref ParseResult intermmediateResult)
        {
            int index = document.FullText.IndexOf('\n');
            string firstLine = document.FullText.Substring(0, index);
            Dictionary<string, HashSet<string>> result = new Dictionary<string, HashSet<string>>();
            List<string> to_match = new List<string>();
            List<string> versions = new List<string> { "eal", "global_platfom", "java_card", "sha", "rsa", "ecc", "des" };
            string eal = @"EAL(\d|\s\d)(\+)?";
            string gp = @"Global(\s)?Platform \d(\.\d)*";
            string jc = @"Java Card \d(\.\d)*";
            string sha = @"SHA(-|\s)?\d{1,3}";
            string rsa = @"RSA((\s|-|_)?\d{3,4}(\/\d{3,4})?|-CRT|SignaturePKCS1|SSA-PSS)";
            string ecc = @"ECC(\s|-)?\d{0,4}?";
            string des = @"(single|3|Triple|T)?(\s|-)?DES\d?";

            to_match.AddRange(new List<string>{eal, gp, jc, sha, rsa, ecc, des});
            int i = 0;
            foreach (string pattern in to_match)
            {


                var matches = Regex.Matches(document.FullText, pattern);
                //For triple-des
                if (i == 6)  matches = Regex.Matches(document.FullText, pattern, RegexOptions.IgnoreCase);
                var matches_string = new HashSet<string>();
                foreach (Match match in matches)
                {
                    matches_string.Add(match.Value);
                }
                if (matches_string.Count != 0) result.Add(versions[i], matches_string);
                i++;
            }

            intermmediateResult.Versions = result;
            /*Versions to do:
             * eal
             * global_platform
             * java_card
             * sha
             * rsa
             * ecc
             * des*/
        }
    }
}
