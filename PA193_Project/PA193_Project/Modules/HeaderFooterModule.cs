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
            current = new string(current.Reverse().ToArray());
            int test = 0;
            //extend the string by one char and check, if this pattern appers enough (quite random percentage) times in the whole document
            while (Regex.Matches(FullTextCopy, current).Count > (collections.Count *30 / 100))
            {
                current = new string(current.Reverse().ToArray());
                i--;
                if (i > FullTextCopy.Length - 1)
                {
                    return;
                }

                //jump over digit, if previous character is also a digit, e.g. to match both 9/100 nad 50/100 etc...
                if (Char.IsDigit(FullTextCopy[i]) && Char.IsDigit(FullTextCopy[i - 1])) continue;

    
                    switch (FullTextCopy[i])
                {
                    case '(':
                        current += +FullTextCopy[i] + "\\";
                        break;
                    case ')':
                        current += FullTextCopy[i] + "\\" ;
                        break;
                    case Char c when Char.IsDigit(c):
                        current += "+d\\";
                        break;
                    default :
                        current += FullTextCopy[i];
                        break;
                }

                    
                current = new string(current.Reverse().ToArray());
                test = Regex.Matches(FullTextCopy, current).Count;
            }
            //remove the char, tha breaks the margin
            current = new string(current.Reverse().ToArray());
            string footer = current.Remove(current.Length - 1);


            var orig_collections = Regex.Matches(document.FullText, @"");

            //Reverse, so I dont mess up the indexes by removing text from the start
            foreach (var match in orig_collections.Reverse())
            {
                int text_index = match.Index;
                int index_diff = 0;
                bool is_match = true;
                int j;


                //hopefuly -2
                for (j = 0; j <= text_index; j++)
                {
                    //document length check
                    if (text_index - j == 0) break;

                    //Check for whitespace, so comparison is valid with the non-whitespace version

                    while (Char.IsWhiteSpace(document.FullText[text_index - j]))
                    {
                        j++;
                        index_diff++;
                        continue;
                    }
                    //check for already full match
                    if (j - index_diff + 1 >= footer.Length - 1)
                    {
                        break;
                    }
                    char current_char = footer[j - index_diff + 1];
                    if (document.FullText[text_index - j] != current_char)
                    {

                        //Number matching case, header has \d+ and real text some numbers
                        if (Char.IsNumber(document.FullText[text_index - j]) && current_char == '\\' && footer[j - index_diff + 2] == 'd')
                        {
                            //set header behind \d+ and cycle throuh the number. Both should end behind the number
                            index_diff -= 3;
                            while (Char.IsNumber(document.FullText[text_index - j]))
                            {
                                j++;
                                //lower index diff so comparison for header does not break in case of big numbers
                                index_diff++;
                            }

                            //This evens out the difference in case of whitespaces after numbers.
                            while (Char.IsWhiteSpace(document.FullText[text_index - j]))
                            {
                                j++;
                                //lower index diff so comparison for header does not break in case of big numbers
                                index_diff++;
                            }

                            //TODO: Bracket parsing

                        }
                        //Handle brackets and maybe others in the future
                        else if (new[] { '(', ')' }.Contains(document.FullText[text_index - j]) && (current_char == '\\')
                            && (new[] { '(', ')' }.Contains(footer[j - index_diff + 2])))
                        {
                            //move header forward and also full text behind the bracket
                            index_diff -= 1;
                            j++;
                        }
                        //In case the text is really different and should not be matched further
                        else
                        {
                            char a = document.FullText[text_index - j];
                            char b = footer[j - index_diff + 1];

                            //As the matching should be at least as long as the header text, take all longer as ok 
                            if (j < footer.Length)
                            {
                                is_match = false;
                            }
                            break;
                        }
                    }

                }

                //If this part of document with the symbol matches header, remove it 
                if (is_match)
                {
                    int p = document.FullText.Length;
                    if (j > 0)
                    {
                        document.FullText = document.FullText.Remove(text_index + 1-j, j - 1);
                    }
                }
            }
            Console.WriteLine(document.FullText);














        //Go down (higher index) to find matches
        i = collections[5].Index;


            current = "" + FullTextCopy[i];
            test = 0;
            //extend the string by one char and check, if this pattern appers enough (quite random percentage) times in the whole document
            while (Regex.Matches(FullTextCopy, current).Count > ((collections.Count * 30 ) / 100))
            {
                int al = Regex.Matches(FullTextCopy, current).Count;
                int mon = (collections.Count / 100) *30;
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

                int pocet = Regex.Matches(FullTextCopy, current).Count;
                 test = Regex.Matches(FullTextCopy, current).Count;
            }
             string header = current.Remove(current.Length -1);


            orig_collections = Regex.Matches(document.FullText, @"");

            //Reverse, so I dont mess up the indexes by removing text from the start
            foreach (var match in orig_collections.Reverse())
            {
                int text_index = match.Index;
                int index_diff = 0;
                bool is_match = true;
                int j;


                //hopefuly -2
                for (j = 0; j < document.FullText.Length -text_index -2; j++)
                {
                    //document length check
                    if (text_index + j >= document.FullText.Length - 1) break;
                    
                    //Check for whitespace, so comparison is valid with the non-whitespace version

                    while (Char.IsWhiteSpace(document.FullText[text_index+ j])) {
                        j++;
                        index_diff++;
                        continue;
                    }
                    //check for already full match
                    if (j-index_diff +1 >= header.Length -1 )
                    {
                        break;
                    }
                    char current_char = header[j - index_diff + 1];
                    if (document.FullText[text_index + j] != current_char) {

                        //Number matching case, header has \d+ and real text some numbers
                        if (Char.IsNumber(document.FullText[text_index + j]) && current_char == '\\' && header[j - index_diff + 2] == 'd')
                        {
                            //set header behind \d+ and cycle throuh the number. Both should end behind the number
                            index_diff -= 3;
                            while (Char.IsNumber(document.FullText[text_index + j]))
                            {
                                j++;
                                //lower index diff so comparison for header does not break in case of big numbers
                                index_diff++;
                            }

                            //This evens out the difference in case of whitespaces after numbers.
                            while (Char.IsWhiteSpace(document.FullText[text_index + j]))
                            {
                                j++;
                                //lower index diff so comparison for header does not break in case of big numbers
                                index_diff++;
                            }

                            //TODO: Bracket parsing

                        }
                        //Handle brackets and maybe others in the future
                        else if(new[] { '(', ')'  }.Contains(document.FullText[text_index+j]) && (current_char == '\\') 
                            && (new[] { '(', ')' }.Contains(header[j - index_diff + 2])))
                        {
                            //move header forward and also full text behind the bracket
                            index_diff -= 1;
                            j++;
                        }
                        //In case the text is really different and should not be matched further
                        else
                        {
                            char a = document.FullText[text_index + j];
                            char b = header[j - index_diff + 1];

                            //As the matching should be at least as long as the header text, take all longer as ok 
                            if (j < header.Length)
                            {
                                is_match = false;
                            }
                            break;
                        }
                    }
                    
                }

                //If this part of document with the symbol matches header, remove it 
                if (is_match)
                {
                    int p = document.FullText.Length;
                    if (j > 0)
                    {
                        document.FullText = document.FullText.Remove(text_index + 1, j - 1);
                    }
                }
            }
            Console.WriteLine(document.FullText);
        }


    }
}
