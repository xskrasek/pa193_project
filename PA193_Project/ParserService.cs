using PA193_Project.Entities;
using PA193_Project.Modules;

namespace PA193_Project.Services
{
    delegate void ModuleChain(Document document, ref ParseResult intermmediateResult);

    class ParserService : IParserService
    {
        private ModuleChain _moduleChain; 

        public ParseResult Parse(Document document)
        {
            ParseResult finalResult = new ParseResult();
            _moduleChain(document, ref finalResult);
            return finalResult;
        }

        public void RegisterModule(IModule module) => _moduleChain += module.Extract;
    }
}
