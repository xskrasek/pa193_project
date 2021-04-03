using System;
using PA193_Project.Entities;

namespace PA193_Project.Modules
{
    class TitleModule : IModule
    {
        public void Extract(Document document, ref ParseResult intermmediateResult)
        {
            // TODO extracts the first line for now (testing purposes)
            int index = document.FullText.IndexOf('\n');
            string firstLine = document.FullText.Substring(0, index);
            intermmediateResult.Title = firstLine;
        }
    }
}
