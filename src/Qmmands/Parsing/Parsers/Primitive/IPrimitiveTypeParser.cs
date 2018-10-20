namespace Qmmands
{
    internal interface IPrimitiveTypeParser
    {
        bool TryParse(CommandService service, string value, out object result);
    }
}
