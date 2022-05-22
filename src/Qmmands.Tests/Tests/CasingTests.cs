using NUnit.Framework;

namespace Qmmands.Tests
{
    public class CasingTests : QmmandsFixture
    {
        private (string, string[])[] ExpectedAndValues =
        {
            ("hello-world",
                new[] { "HelloWorld", "hello - world", "hello_world" }),

            ("1-cheese",
                new[] { "1cheese" }),

            ("helloworld",
                new[] { "helloworld" }),

            ("hello-wo-rl-d",
                new[] { "Hello WoRlD" }),

            ("hello-123-world",
                new[] { "hello123world" }),

            ("com-object",
                new[] { "COMObject" }),

            ("web-api",
                new[] { "WebAPI" }),

            ("windows-server-2016-r-2",
                new[] { "WindowsServer2016R2" })
        };

        [Test]
        public void KebabCase()
        {
            foreach (var (expected, values) in ExpectedAndValues)
            {
                foreach (var value in values)
                {
                    var actual = CommandUtilities.ToKebabCase(value);

                    Assert.AreEqual(expected, actual);
                }
            }
        }
    }
}
