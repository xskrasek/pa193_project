using PA193_Project.Entities;

namespace PA193_Project.Modules
{
    interface IModule
    {
        void Extract(Document document, ref ParseResult intermmediateResult);
    }
}
