using System;
using PA193_Project.Entities;
using System;
using System.Text;

namespace PA193_Project.Modules
{
	class BibliographyModule : IModule
	{

		private static void findBibliography()
		{
			int startingLine = 0, endingLine = 0, temp = 0;
			StreamReader reader = new StreamReader(new FileStream(inputFile, FileMode.Open, FileAccess.Read));
			for (int i = 1; i < numberOfLines + 1; ++i)
			{
				string currentLine = reader.readLine();
				Pattern pattern = Pattern.compile("(Bibliography)|(BIBLIOGRAPHY)|(INDEX)");
				Matcher matcher = pattern.matcher(currentLine);

				while (matcher.matches())
				{
					startingLine = i;
					break;
				}
			}
			StreamReader reader2 = new StreamReader(new FileStream(inputFile, FileMode.Open, FileAccess.Read));
			string currentLineN = "";
			for (int i = 1; i < numberOfLines + 1; ++i)
			{
				currentLineN = reader2.readLine();
				Pattern pattern = Pattern.compile("(Bibliography)|(BIBLIOGRAPHY)|(INDEX)");
				Matcher matcher = pattern.matcher(currentLineN);

				while (matcher.find())
				{
					temp = i;
					currentLineN = reader2.readLine();
					break;
				}
			}
			string[] nextHeading = currentLineN.Split(" ", true);
			string nextFindHeading = nextHeading[0];

			StreamReader reader3 = new StreamReader(new FileStream(inputFile, FileMode.Open, FileAccess.Read));
			for (int i = 1; i < numberOfLines + 1; ++i)
			{
				string currentLine = reader3.readLine();
				Pattern pattern = Pattern.compile(nextFindHeading);
				Matcher matcher = pattern.matcher(currentLine);

				while (matcher.matches() && temp != i)
				{
					endingLine = i;
					break;
				}
			}
			StreamReader reader4 = new StreamReader(new FileStream(inputFile, FileMode.Open, FileAccess.Read));
			for (int i = 1; i < numberOfLines + 1; ++i)
			{
				string currentLine = reader4.readLine();
				while (i >= startingLine && i < endingLine)
				{

					Console.WriteLine(currentLine);
				}
			}

		}

		

		internal static class StringHelper
		{
			
			public static bool StartsWith(this string self, string prefix, int toffset)
			{
				return self.IndexOf(prefix, toffset, StringComparison.Ordinal) == toffset;
			}

			
			public static string[] Split(this string self, string regexDelimiter, bool trimTrailingEmptyStrings)
			{
				string[] splitArray = RegularExpressions.Regex.Split(self, regexDelimiter);

				if (trimTrailingEmptyStrings)
				{
					if (splitArray.Length > 1)
					{
						for (int i = splitArray.Length; i > 0; i--)
						{
							if (splitArray[i - 1].Length > 0)
							{
								if (i < splitArray.Length)
									Array.Resize(ref splitArray, i);

								break;
							}
						}
					}
				}

				return splitArray;
			}

			
			public static string NewString(sbyte[] bytes)
			{
				return NewString(bytes, 0, bytes.Length);
			}
			public static string NewString(sbyte[] bytes, int index, int count)
			{
				return Encoding.UTF8.GetString((byte[])(object)bytes, index, count);
			}
			public static string NewString(sbyte[] bytes, string encoding)
			{
				return NewString(bytes, 0, bytes.Length, encoding);
			}
			public static string NewString(sbyte[] bytes, int index, int count, string encoding)
			{
				return NewString(bytes, index, count, Encoding.GetEncoding(encoding));
			}
			public static string NewString(sbyte[] bytes, Encoding encoding)
			{
				return NewString(bytes, 0, bytes.Length, encoding);
			}
			public static string NewString(sbyte[] bytes, int index, int count, Encoding encoding)
			{
				return encoding.GetString((byte[])(object)bytes, index, count);
			}

			
			public static sbyte[] GetBytes(this string self)
			{
				return GetSBytesForEncoding(Encoding.UTF8, self);
			}
			public static sbyte[] GetBytes(this string self, Encoding encoding)
			{
				return GetSBytesForEncoding(encoding, self);
			}
			public static sbyte[] GetBytes(this string self, string encoding)
			{
				return GetSBytesForEncoding(Encoding.GetEncoding(encoding), self);
			}
			private static sbyte[] GetSBytesForEncoding(Encoding encoding, string s)
			{
				sbyte[] sbytes = new sbyte[encoding.GetByteCount(s)];
				encoding.GetBytes(s, 0, s.Length, (byte[])(object)sbytes, 0);
				return sbytes;
			}
		}
	}
}