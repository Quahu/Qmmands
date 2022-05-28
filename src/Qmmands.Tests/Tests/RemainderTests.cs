using System;
using System.Globalization;
using System.Reflection;
using NUnit.Framework;
using Qmmands.Text;
using Qmmands.Text.Default;

namespace Qmmands.Tests;

public class RemainderTests : QmmandsFixture
{
    public class RemainderTestModule : TextModuleBase
    {
        [TextCommand("echo")]
        public void Echo(string text)
        { }

        [TextCommand("echo-remainder")]
        public void EchoRemainder([Remainder] string text)
        { }
    }

    /// <inheritdoc />
    public override void Setup()
    {
        base.Setup();

        CommandService.AddModule(typeof(RemainderTestModule).GetTypeInfo());
    }

    [Test]
    public void Test()
    {
        var textContext = new DefaultTextCommandContext(Services, CultureInfo.InvariantCulture, default);

        textContext.InputString = "echo a b c".AsMemory();
        var result = CommandService.ExecuteAsync(textContext).Result;
        Assert.IsFalse(result.IsSuccessful);

        textContext.ResetAsync();

        textContext.InputString = "echo-remainder a b c".AsMemory();
        result = CommandService.ExecuteAsync(textContext).Result;
        Assert.IsTrue(result.IsSuccessful);
    }
}
