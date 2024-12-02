using NF.Tool.ReleaseNoteMaker.CLI.Commands;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Threading.Tasks;

namespace NF.Tool.ReleaseNoteMaker.CLI
{
    internal sealed class Program
    {
#pragma warning disable IDE0210 // Convert to top-level statements
        internal static async Task<int> Main(string[] args)
        {
            CommandApp app = new CommandApp();

            app.Configure(config =>
            {
                _ = config.PropagateExceptions();
                _ = config.SetApplicationName("release-note-maker");

                _ = config.AddCommand<Command_Init>("init")
                    .WithExample("init");
                _ = config.AddCommand<Command_Create>("create")
                    .WithExample("create", "--edit")
                    .WithExample("create", "1.added.md", "--content", @"""Hello World""");
                _ = config.AddCommand<Command_Preview>("preview");
                _ = config.AddCommand<Command_Build>("build")
                    .WithExample("build --version 1.0.0");
                _ = config.AddCommand<Command_Check>("check")
                    .WithExample("check");
            });

#pragma warning disable CA1031 // Do not catch general exception types
            try
            {
                return await app.RunAsync(args);
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
                return 1;
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }
#pragma warning restore IDE0210 // Convert to top-level statements
    }
}
