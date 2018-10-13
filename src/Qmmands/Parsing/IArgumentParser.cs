namespace Qmmands
{
    /// <summary>
    ///     The default interface for raw argument parsers. You'd rather not touch this.
    /// <para>
    /// ​
    /// </para>
    /// <para>
    /// ​
    /// </para>
    /// <para>
    ///     Trust me.
    /// </para>
    /// </summary>
    public interface IArgumentParser
    {
        /// <summary>
        ///     No documentation, I'm done. No sane person would even try to use this anyways.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="rawArguments"></param>
        /// <returns></returns>
        ParseResult ParseRawArguments(Command command, string rawArguments);
    }
}
