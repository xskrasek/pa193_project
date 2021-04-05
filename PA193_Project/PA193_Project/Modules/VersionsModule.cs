using PA193_Project.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PA193_Project.Modules
{
    class VersionsModule : IModule
    {
        public void Extract(Document document, ref ParseResult intermmediateResult)
        {
            int index = document.FullText.IndexOf('\n');
            string firstLine = document.FullText.Substring(0, index);
            Dictionary<string, List<string>> result = new Dictionary<string, List<string>>
            {
                { "HELLO", new List<string>() {"WORLD" } }
            };
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
