using System.Collections.Generic;
using PA193_Project.Entities;
using PA193_Project.Modules;

namespace PA193_Project.Services
{
    interface IParserService
    {
        ParseResult Parse(Document document);
        public void RegisterModule(IModule module);
    }
}
