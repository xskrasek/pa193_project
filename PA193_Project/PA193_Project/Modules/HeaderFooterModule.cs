using PA193_Project.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PA193_Project.Modules
{
    class HeaderFooterModule : IModule
    {
        public void Extract(Document document, ref ParseResult intermmediateResult)
        {
            var collections = Regex.Matches(document.FullText, @"");

            if (collections.Count() <= 5 )
            {
                Console.WriteLine("Not enough data for header/footer removal");
            }
            int i = collections[5].Index;



        }
    }
}
