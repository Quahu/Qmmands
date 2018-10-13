namespace Qmmands
{
    internal interface IPrimitiveTypeParser
    {
        bool TryParse(string value, out object result);
    }
}
