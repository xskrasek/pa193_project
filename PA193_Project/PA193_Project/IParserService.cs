using System.Collections.Generic;
using PA193_Project.Entities;

namespace PA193_Project.Services
{
    interface IParserService
    {
        ParseResult Parse(Document document);
    }
}
