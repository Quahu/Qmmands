using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Serilog;
using Serilog.Core;
using Serilog.Extensions.Logging;
using Serilog.Sinks.SystemConsole.Themes;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Qmmands.Tests
{
    [FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
    public abstract unsafe class QmmandsFixture
    {
        protected ILogger Logger = null!;

        protected TestContext _context = null!;

        protected static readonly ILoggerFactory LoggerFactory;
        protected static readonly IServiceProvider StaticServices;

        protected IServiceProvider Services = null!;
        private static readonly Logger _serilogLogger;

        static QmmandsFixture()
        {
            _serilogLogger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code, outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            LoggerFactory = new SerilogLoggerFactory(_serilogLogger);
            StaticServices = new ServiceCollection()
                .BuildServiceProvider();
        }

        [SetUp]
        public virtual void Setup()
        {
            _context = TestContext.CurrentContext;

            Logger = LoggerFactory.CreateLogger(_context.Test.Name);

            var services = new ServiceCollection()
                .AddOptions()
                .AddLogging(x => x.AddSerilog(_serilogLogger));

            ServiceSetup(services);
            Services = services.BuildServiceProvider();
        }

        protected virtual void ServiceSetup(IServiceCollection services)
        { }

        [TearDown]
        public virtual void Teardown()
        { }
    }
}
