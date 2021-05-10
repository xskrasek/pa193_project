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
            var fullTextCopy = string.Concat(document.FullText.Where(c => !char.IsWhiteSpace(c) || c == ''));

            //Reverse text, so search is the same as for header
            var reverseFullText = new string(document.FullText.Reverse().ToArray());
            //var ReverseFulltext = reverseFullText.Replace("\n", "n\\");
            var collections = Regex.Matches(fullTextCopy, @"");

            if (collections.Count() <= 5)
            {
                Console.WriteLine("Not enough data for header/footer removal");
                return;
            }

            var i = collections[5].Index;
            //Go up (lower index) to find matches

            var current = "" + fullTextCopy[i];
            //extend the string by one char and check, if this pattern appears enough (quite random percentage) times in the whole document
            while (Regex.Matches(fullTextCopy, current).Count > collections.Count * 45 / 100)
            {
                i--;
                if (i > fullTextCopy.Length - 1) return;

                //jump over digit, if previous character is also a digit, e.g. to match both 9/100 nad 50/100 etc...
                if (char.IsDigit(fullTextCopy[i]) && char.IsDigit(fullTextCopy[i - 1])) continue;


                current = fullTextCopy[i] switch
                {
                    '+' => "\\" + fullTextCopy[i] + current,
                    '|' => "\\" + fullTextCopy[i] + current,
                    '(' => "\\" + fullTextCopy[i] + current,
                    ')' => "\\" + fullTextCopy[i] + current,
                    '.' => "\\." + current,
                    var c when char.IsDigit(c) => "\\d+" + current,
                    _ => fullTextCopy[i] + current
                };

            }

            //remove the char, that breaks the margin, if its special char, two removes two so its ok
            if (current[0] == '\\') current = current.Remove(0, 1);
            var footer = current.Remove(0, 1);

            footer = ReverseRegex(footer);
            var origCollections = Regex.Matches(reverseFullText, @"");

            //Reverse, so I dont mess up the indexes by removing text from the start
            foreach (var match in origCollections.Reverse())
            {
                var textIndex = match.Index;
                var indexDiff = 0;
                var isMatch = true;
                int j;
                int whitespaceOffset = -1;


                //hopefuly -2
                for (j = 0; j < reverseFullText.Length - textIndex - 2; j++)
                {
                    //document length check
                    if (textIndex + j >= reverseFullText.Length - 1 || reverseFullText.Length < j) break;

                    //Check for whitespace, so comparison is valid with the non-whitespace version

                    while (char.IsWhiteSpace(reverseFullText[textIndex + j]))
                    {
                        j++;
                        indexDiff++;
                        if (j - indexDiff + 1 >= footer.Length) whitespaceOffset++;
                    }

                    //check for already full match
                    if (j - indexDiff + 1 >= footer.Length) break;
                    var currentChar = footer[j - indexDiff + 1];
                    var textChar = reverseFullText[textIndex + j];
                    if (reverseFullText[textIndex + j] != currentChar)
                    {
                        //Number matching case, header has \d+ and real text some numbers
                        if (char.IsNumber(reverseFullText[textIndex + j]) && currentChar == '\\' &&
                            footer[j - indexDiff + 2] == 'd')
                        {
                            //set header behind \d and cycle throuh the number. 
                            indexDiff -= 3;
                            while (char.IsNumber(reverseFullText[textIndex + j]))
                            {
                                j++;
                                //lower index diff so comparison for header does not break in case of big numbers
                                indexDiff++;
                            }

                            j--;
                        }
                        //Handle brackets and maybe others in the future
                         else if (new[] {'(', ')', '|' , '+' }.Contains(reverseFullText[textIndex + j]) && currentChar == '\\'
                            && new[] {'(', ')', '|' , '+' }.Contains(footer[j - indexDiff + 2]) && reverseFullText[textIndex+j] == footer[j-indexDiff+2])
                        {
                            //move header forward and also full text behind the bracket
                            indexDiff--;
                            //j++;
                        }
                        //In case the text is really different and should not be matched further
                        
                        else
                        {
                            currentChar = footer[j - indexDiff + 1];
                            textChar = reverseFullText[textIndex + j];
                            //As the matching should be at least as long as the header text, take all longer as ok 
                            if (j < footer.Length) isMatch = false;
                            break;
                        }
                    }
                }

                //If this part of document with the symbol matches header, remove it 
                if (isMatch)
                {
                    var p = reverseFullText.Length;
                    if (j > 0)
                        //no +1 here, we need to keep the symbol alive
                    if (j-2-whitespaceOffset >0) reverseFullText = reverseFullText.Remove(textIndex + 1, j - 2 - whitespaceOffset);
                }
            }

            var reverseBack = new string(reverseFullText.Reverse().ToArray());
            document.FullText = reverseBack.Replace("n\\", "\n");
            //Console.WriteLine(document.FullText);


            //Go down (higher index) to find matches
            i = collections[5].Index;


            var matchCount = 0;
            int colInc = 0;
            string prevMatch = "";
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
                while (Regex.Matches(fullTextCopy, current).Count > collections.Count * 40 / 100)
                {
                    var al = Regex.Matches(fullTextCopy, current).Count;
                    var mon = collections.Count / 100 * 30;
                    i++;
                    if (i > fullTextCopy.Length - 1) return;

                    //jump over digit, if previous character is also a digit, e.g. to match both 9/100 nad 50/100 etc...
                    if (char.IsDigit(fullTextCopy[i]) && char.IsDigit(fullTextCopy[i - 1])) continue;

                    current += fullTextCopy[i] switch
                    {
                        '+' => "\\" + fullTextCopy[i],
                        '[' => "\\" + fullTextCopy[i],
                        ']' => "\\" + fullTextCopy[i] ,
                        '|' => "\\" + fullTextCopy[i],
                        '(' => "\\" + fullTextCopy[i],
                        ')' => "\\" + fullTextCopy[i],
                        var c when char.IsDigit(c) => "\\d+",
                        _ => fullTextCopy[i],
                    };
                    var pocet = Regex.Matches(fullTextCopy, current).Count;
                }

                if (current[current.Length - 1] == '+') current = current.Remove(current.Length - 1);
                if (current[current.Length - 1] == 'd') current = current.Remove(current.Length - 1);
                var header = current.Remove(current.Length - 1);

                if (header.Length < 6) continue;

                if (!String.Equals(header,prevMatch))
                {
                    prevMatch = header;
                matchCount++;
                     }


                origCollections = Regex.Matches(document.FullText, @"");

                //Reverse, so I dont mess up the indexes by removing text from the start
                foreach (var match in origCollections.Reverse())
                {
                    var textIndex = match.Index;
                    var indexDiff = 0;
                    var isMatch = true;
                    int j;


                    //hopefuly -2
                    for (j = 0; j < document.FullText.Length - textIndex - 2; j++)
                    {
                        //document length check
                        if (textIndex + j >= document.FullText.Length - 1) break;

                        //Check for whitespace, so comparison is valid with the non-whitespace version

                        while (char.IsWhiteSpace(document.FullText[textIndex + j]))
                        {
                            j++;
                            indexDiff++;
                        }

                        //check for already full match
                        if (j - indexDiff + 1 >= header.Length - 1) break;
                        var currentChar = header[j - indexDiff + 1];
                        if (document.FullText[textIndex + j] != currentChar)
                        {
                            //Number matching case, header has \d+ and real text some numbers
                            if (char.IsNumber(document.FullText[textIndex + j]) && currentChar == '\\' &&
                                header[j - indexDiff + 2] == 'd')
                            {
                                //set header behind \d+ and cycle throuh the number. Both should end behind the number
                                indexDiff -= 3;
                                while (char.IsNumber(document.FullText[textIndex + j]))
                                {
                                    j++;
                                    //lower index diff so comparison for header does not break in case of big numbers
                                    indexDiff++;
                                }

                                //This evens out the difference in case of whitespaces after numbers.
                                while (char.IsWhiteSpace(document.FullText[textIndex + j]))
                                {
                                    j++;
                                    //lower index diff so comparison for header does not break in case of big numbers
                                    indexDiff++;
                                }
                            }
                            //Handle brackets and maybe others in the future
                            else if (new[] {'(', ')', '|' , '[' , ']', '+' }.Contains(document.FullText[textIndex + j]) &&
                                     currentChar == '\\'
                                     && new[] {'(', ')', '|' , '[' , ']', '+' }.Contains(header[j - indexDiff + 2]))
                            {
                                //move header forward and also full text behind the bracket
                                indexDiff -= 1;
                                j++;
                            }
                            //In case the text is really different and should not be matched further
                            else
                            {
                                var a = document.FullText[textIndex + j];
                                var b = header[j - indexDiff + 1];

                                //As the matching should be at least as long as the header text, take all longer as ok 
                                if (j < header.Length) isMatch = false;
                                break;
                            }
                        }
                    }

                    //If this part of document with the symbol matches header, remove it 
                    if (isMatch)
                    {
                        var p = document.FullText.Length;
                        if (j > 0) document.FullText = document.FullText.Remove(textIndex + 1, j - 2);
                    }
                }
            }

            //Console.WriteLine(document.FullText);
        }
    }
}