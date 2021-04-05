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
            List<string> to_find_behind = new List<string>{ "eal", "global_platfom", "java_card", "sha", "rsa", "ecc", "des" };

            foreach (string version in to_find_behind)
            {
                var matches = Regex.Matches(document.FullText, $"{version.ToUpper()}[0-9]*[-]*[0-9]*");
                var matches_string = new HashSet<string>();
                foreach (Match match in matches)
                {
                    matches_string.Add(match.Value);
                }

                result.Add(version, matches_string);

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
