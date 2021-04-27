using System;
using System.Linq;
using System.Text.RegularExpressions;
using PA193_Project.Entities;


namespace PA193_Project.Modules

{


    internal class HeaderFooterModule : IModule


    {
        string ReverseRegex(string regex)
        {
            string res = "";
            for (int i = 0; i < regex.Length; i++)
            {
                if (regex[i] == '\\')
                {
                    if (regex[i + 1] == 'd')
                    {
                        if (i + 2 < regex.Length)
                        {
                            if (regex[i + 2] == '+')
                            {
                                res += "+d\\";
                                i += 2;
                                continue;
                                
                            }
                        }

                        res += "d\\";
                        i += 1;
                    }
                    else
                    {
                        res +=  regex[i+1] + "\\";
                        i += 1;
                    }
                }
                else
                {
                    res += regex[i];
                }
            }

            return new string(res.Reverse().ToArray());
        }

        public void Extract(Document document, ref ParseResult intermmediateResult)
        {
            var FullTextCopy = string.Concat(document.FullText.Where(c => !char.IsWhiteSpace(c) || c == ''));

            //Reverse text, so search is the same as for header
            var ReverseFullText = new string(document.FullText.Reverse().ToArray());
            var ReverseFulltext = ReverseFullText.Replace("\n", "n\\");
            var collections = Regex.Matches(FullTextCopy, @"");

            if (collections.Count() <= 5)
            {
                Console.WriteLine("Not enough data for header/footer removal");
                return;
            }

            var i = collections[5].Index;
            //Go up (lower index) to find matches

            var current = "" + FullTextCopy[i];
            //extend the string by one char and check, if this pattern appears enough (quite random percentage) times in the whole document
            while (Regex.Matches(FullTextCopy, current).Count > collections.Count * 45 / 100)
            {
                i--;
                if (i > FullTextCopy.Length - 1) return;

                //jump over digit, if previous character is also a digit, e.g. to match both 9/100 nad 50/100 etc...
                if (char.IsDigit(FullTextCopy[i]) && char.IsDigit(FullTextCopy[i - 1])) continue;


                current = FullTextCopy[i] switch
                {
                    '+' => "\\" + FullTextCopy[i] + current,
                    '|' => "\\" + FullTextCopy[i] + current,
                    '(' => "\\" + FullTextCopy[i] + current,
                    ')' => "\\" + FullTextCopy[i] + current,
                    '.' => "\\." + current,
                    char c when char.IsDigit(c) => "\\d+" + current,
                    _ => FullTextCopy[i] + current
                };

            }

            //remove the char, that breaks the margin, if its special char, two removes two so its ok
            if (current[0] == '\\') current = current.Remove(0, 1);
            var footer = current.Remove(0, 1);

            footer = ReverseRegex(footer);
            var orig_collections = Regex.Matches(ReverseFullText, @"");

            //Reverse, so I dont mess up the indexes by removing text from the start
            foreach (var match in orig_collections.Reverse())
            {
                var text_index = match.Index;
                var index_diff = 0;
                var is_match = true;
                int j;
                int whitespaceOffset = -1;


                //hopefuly -2
                for (j = 0; j < ReverseFullText.Length - text_index - 2; j++)
                {
                    //document length check
                    if (text_index + j >= ReverseFullText.Length - 1 || ReverseFullText.Length < j) break;

                    //Check for whitespace, so comparison is valid with the non-whitespace version

                    while (char.IsWhiteSpace(ReverseFullText[text_index + j]))
                    {
                        j++;
                        index_diff++;
                        if (j - index_diff + 1 >= footer.Length) whitespaceOffset++;
                    }

                    //check for already full match
                    if (j - index_diff + 1 >= footer.Length) break;
                    var current_char = footer[j - index_diff + 1];
                    var text_char = ReverseFullText[text_index + j];
                    if (ReverseFullText[text_index + j] != current_char)
                    {
                        //Number matching case, header has \d+ and real text some numbers
                        if (char.IsNumber(ReverseFullText[text_index + j]) && current_char == '\\' &&
                            footer[j - index_diff + 2] == 'd')
                        {
                            //set header behind \d and cycle throuh the number. 
                            index_diff -= 3;
                            while (char.IsNumber(ReverseFullText[text_index + j]))
                            {
                                j++;
                                //lower index diff so comparison for header does not break in case of big numbers
                                index_diff++;
                            }

                            /*//This evens out the difference in case of whitespaces after numbers.
                            while (char.IsWhiteSpace(ReverseFullText[text_index + j]))
                            {
                                j++;
                                //lower index diff so comparison for header does not break in case of big numbers
                                index_diff++;
                            }*/
                            j--;
                        }
                        //Handle brackets and maybe others in the future
                         else if (new[] {'(', ')', '|' , '+' }.Contains(ReverseFullText[text_index + j]) && current_char == '\\'
                            && new[] {'(', ')', '|' , '+' }.Contains(footer[j - index_diff + 2]) && ReverseFullText[text_index+j] == footer[j-index_diff+2])
                        {
                            //move header forward and also full text behind the bracket
                            index_diff--;
                            //j++;
                        }
                        //In case the text is really different and should not be matched further
                        
                        else
                        {
                            current_char = footer[j - index_diff + 1];
                            text_char = ReverseFullText[text_index + j];
                            //As the matching should be at least as long as the header text, take all longer as ok 
                            if (j < footer.Length) is_match = false;
                            break;
                        }
                    }
                }

                //If this part of document with the symbol matches header, remove it 
                if (is_match)
                {
                    var p = ReverseFullText.Length;
                    if (j > 0)
                        //no +1 here, we need to keep the symbol alive
                    if (j-2-whitespaceOffset >0) ReverseFullText = ReverseFullText.Remove(text_index + 1, j - 2 - whitespaceOffset);
                }
            }

            var ReverseBack = new string(ReverseFullText.Reverse().ToArray());
            document.FullText = ReverseBack.Replace("n\\", "\n");
            //Console.WriteLine(document.FullText);


            //Go down (higher index) to find matches
            i = collections[5].Index;


            var matchCount = 0;
            int colInc = 0;
            string prev_match = "";
            while (matchCount < 2)
            {
                current = "" + "\f";

                //move to another possible pattern
                //Try some other matches
                if (colInc + 6 == collections.Count - 1)
                {
                    break;
                }
                if (matchCount == 1) i = collections[6+colInc].Index;
                colInc++;
                //extend the string by one char and check, if this pattern appers enough (quite random percentage) times in the whole document
                while (Regex.Matches(FullTextCopy, current).Count > collections.Count * 40 / 100)
                {
                    var al = Regex.Matches(FullTextCopy, current).Count;
                    var mon = collections.Count / 100 * 30;
                    i++;
                    if (i > FullTextCopy.Length - 1) return;

                    //jump over digit, if previous character is also a digit, e.g. to match both 9/100 nad 50/100 etc...
                    if (char.IsDigit(FullTextCopy[i]) && char.IsDigit(FullTextCopy[i - 1])) continue;

                    current += FullTextCopy[i] switch
                    {
                        '+' => "\\" + FullTextCopy[i],
                        '[' => "\\" + FullTextCopy[i],
                        ']' => "\\" + FullTextCopy[i] ,
                        '|' => "\\" + FullTextCopy[i],
                        '(' => "\\" + FullTextCopy[i],
                        ')' => "\\" + FullTextCopy[i],
                        char c when char.IsDigit(c) => "\\d+",
                        _ => FullTextCopy[i],
                    };
                    var pocet = Regex.Matches(FullTextCopy, current).Count;
                }

                if (current[current.Length - 1] == '+') current = current.Remove(current.Length - 1);
                if (current[current.Length - 1] == 'd') current = current.Remove(current.Length - 1);
                var header = current.Remove(current.Length - 1);

                if (header.Length < 6) continue;

                if (!String.Equals(header,prev_match))
                {
                    prev_match = header;
                matchCount++;
                     }


                orig_collections = Regex.Matches(document.FullText, @"");

                //Reverse, so I dont mess up the indexes by removing text from the start
                foreach (var match in orig_collections.Reverse())
                {
                    var text_index = match.Index;
                    var index_diff = 0;
                    var is_match = true;
                    int j;


                    //hopefuly -2
                    for (j = 0; j < document.FullText.Length - text_index - 2; j++)
                    {
                        //document length check
                        if (text_index + j >= document.FullText.Length - 1) break;

                        //Check for whitespace, so comparison is valid with the non-whitespace version

                        while (char.IsWhiteSpace(document.FullText[text_index + j]))
                        {
                            j++;
                            index_diff++;
                        }

                        //check for already full match
                        if (j - index_diff + 1 >= header.Length - 1) break;
                        var current_char = header[j - index_diff + 1];
                        if (document.FullText[text_index + j] != current_char)
                        {
                            //Number matching case, header has \d+ and real text some numbers
                            if (char.IsNumber(document.FullText[text_index + j]) && current_char == '\\' &&
                                header[j - index_diff + 2] == 'd')
                            {
                                //set header behind \d+ and cycle throuh the number. Both should end behind the number
                                index_diff -= 3;
                                while (char.IsNumber(document.FullText[text_index + j]))
                                {
                                    j++;
                                    //lower index diff so comparison for header does not break in case of big numbers
                                    index_diff++;
                                }

                                //This evens out the difference in case of whitespaces after numbers.
                                while (char.IsWhiteSpace(document.FullText[text_index + j]))
                                {
                                    j++;
                                    //lower index diff so comparison for header does not break in case of big numbers
                                    index_diff++;
                                }
                            }
                            //Handle brackets and maybe others in the future
                            else if (new[] {'(', ')', '|' , '[' , ']', '+' }.Contains(document.FullText[text_index + j]) &&
                                     current_char == '\\'
                                     && new[] {'(', ')', '|' , '[' , ']', '+' }.Contains(header[j - index_diff + 2]))
                            {
                                //move header forward and also full text behind the bracket
                                index_diff -= 1;
                                j++;
                            }
                            //In case the text is really different and should not be matched further
                            else
                            {
                                var a = document.FullText[text_index + j];
                                var b = header[j - index_diff + 1];

                                //As the matching should be at least as long as the header text, take all longer as ok 
                                if (j < header.Length) is_match = false;
                                break;
                            }
                        }
                    }

                    //If this part of document with the symbol matches header, remove it 
                    if (is_match)
                    {
                        var p = document.FullText.Length;
                        if (j > 0) document.FullText = document.FullText.Remove(text_index + 1, j - 2);
                    }
                }
            }

            //Console.WriteLine(document.FullText);
        }
    }
}