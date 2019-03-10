namespace Qmmands
{
    internal interface IPrimitiveTypeParser
    {
        bool TryParse(Parameter parameter, string value, out object result);
    }
}
