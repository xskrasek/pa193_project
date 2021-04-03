using System.Collections.Generic;
using PA193_Project.Entities;

namespace PA193_Project.Services
{
    interface IParserService
    {
        // TODO The return type is provisionary. We will most likely need an "OutputObject", or something like that.
        Dictionary<string, object> Parse(Document document);
    }
}
