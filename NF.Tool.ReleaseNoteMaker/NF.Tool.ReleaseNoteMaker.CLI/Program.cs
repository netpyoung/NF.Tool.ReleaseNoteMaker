using NF.Tool.ReleaseNoteMaker.CLI.Commands;
using NF.Tool.ReleaseNoteMaker.CLI.Impl;
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
                    .WithExample("init")
                    .WithExample("init", "--file", Const.DEFAULT_CONFIG_FILENAME);
                _ = config.AddCommand<Command_Create>("create")
                    .WithExample("create", "--edit")
                    .WithExample("create", "--content", @"""Hello World""", "1.added.md");
                _ = config.AddCommand<Command_Build>("build")
                   .WithExample("build --version 1.0.0")
                   .WithExample("build --version 1.0.0 --draft");
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
