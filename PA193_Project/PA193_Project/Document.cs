using System;
using System.Collections.Generic;
using System.IO;

namespace PA193_Project.Entities
{
    class Document
    {
        private string _fullText;
        public string FullText
        {
            get => _fullText;
            set
            {
                // Generate page indices
                _indices.Clear();
                for (int i = 0; i < value.Length; i++)
                {
                    if (value[i] == 0xff)
                    {
                        _indices.Add(i);
                    }
                }
                _fullText = value;
            }
        }

        private List<int> _indices;

        public List<int> Indices { get => _indices; }

        public string CleanedText { get; set; }

        private string _filepath;
        public string Filepath
        {
            get => _filepath;
            set
            {
                // Checks for both, if path exists and is a file (as opposed to directory)
                if (!File.Exists(value)) { throw new FileNotFoundException(value); }
                try
                {
                    using var streamReader = new StreamReader(value);
                    this.FullText = streamReader.ReadToEnd();
                }
                catch (IOException)
                {
                    throw; // For now to test global exception handling
                }
                _filepath = value;
            }
        }
        public Document()
        {
            _indices = new List<int>();
        }

        public string GetPage(int pageNumber)
        {
            pageNumber = Math.Clamp(pageNumber, 0, this.Indices.Count);
            int charIndex = this.Indices[pageNumber];
            int previousIndex = pageNumber == 0 ? 0 : this.Indices[pageNumber - 1];
            return this.FullText[previousIndex..charIndex];
        }
    }
}
