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
                _ = config.UseStrictParsing();
                _ = config.SetApplicationName("dotnet release-note");

                _ = config.AddCommand<Command_Init>("init")
                    .WithExample("init");
                _ = config.AddCommand<Command_Create>("create")
                    .WithExample("create", "--edit")
                    .WithExample("create", "1.added.md", "--content", @"""Hello World""");
                _ = config.AddCommand<Command_Preview>("preview");
                _ = config.AddCommand<Command_Build>("build")
                    .WithExample("build --version 1.0.0");
                _ = config.AddCommand<Command_Read>("read")
                    .WithExample("read --version 1.0.0");
                _ = config.AddCommand<Command_Check>("check")
                    .WithExample("check");
            });

            try
            {
                return await app.RunAsync(args);
            }
            catch (CommandAppException ex)
            {
                if (ex.Pretty is { } pretty)
                {
                    AnsiConsole.Write(pretty);
                }
                else
                {
                    AnsiConsole.MarkupInterpolated($"[red]Error:[/] {ex.Message}");
                }
                return 1;
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex, new ExceptionSettings()
                {
                    Format = ExceptionFormats.ShortenEverything,
                    Style = new()
                    {
                        ParameterName = Color.Grey,
                        ParameterType = Color.Grey78,
                        LineNumber = Color.Grey78,
                    },
                });
                return 1;
            }
        }
#pragma warning restore IDE0210 // Convert to top-level statements
    }
}
