using PA193_Project.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
//TODO: Erro handling
namespace PA193_Project.Modules
{
    class HeaderFooterModule : IModule
    {
        public void Extract(Document document, ref ParseResult intermmediateResult)
        {
            string FullTextCopy = String.Concat(document.FullText.Where(c => !Char.IsWhiteSpace(c) || c == ''));

            var collections = Regex.Matches(FullTextCopy, @"");

            if (collections.Count() <= 5 )
            {
                Console.WriteLine("Not enough data for header/footer removal");
                return;
            }

            int i = collections[5].Index;
            //Go up (lower index) to find matches
            
            string current = "" + FullTextCopy[i];

            //extend the string by one char and check, if this pattern appers enough (quite random percentage) times in the whole document
            while (Regex.Matches(FullTextCopy, current).Count > (collections.Count /100 * 30))
            {
                //TODO: String extension
                i--;
                current += FullTextCopy[i];
            }
            string footer = current.Remove(current.Length -1);

            //Go down (higher index) to find matches
            i = collections[5].Index;


            current = "" + FullTextCopy[i];
            int test = 0;
            //extend the string by one char and check, if this pattern appers enough (quite random percentage) times in the whole document
            while (Regex.Matches(FullTextCopy, current).Count > (collections.Count / 100 * 30))
            {
                //TODO: Multi/Digit part
                i++;
                if (i > FullTextCopy.Length - 1)
                {
                    return;
                }

                //jump over digit, if previous character is also a digit, e.g. to match both 9/100 nad 50/100 etc...
                if (Char.IsDigit(FullTextCopy[i]) && Char.IsDigit(FullTextCopy[i - 1])) continue;
     
                    switch (FullTextCopy[i])
                {
                    case '(':
                        current += "\\"+FullTextCopy[i];
                        break;
                    case ')':
                        current += "\\" + FullTextCopy[i];
                        break;
                    case Char c when Char.IsDigit(c):
                        current += "\\d+";
                        break;
                    default :
                        current += FullTextCopy[i];
                        break;
                }

                
                 test = Regex.Matches(FullTextCopy, current).Count;
            }
             string header = current.Remove(current.Length -1);


            //TODO: Go through orig text and match the parts, so the can be removed
        }
    }
}
