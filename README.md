# NF.Tool.ReleaseNoteMaker

[![.NET Test Workflow](https://github.com/netpyoung/NF.Tool.ReleaseNoteMaker/actions/workflows/dotnet-test.yml/badge.svg)](https://github.com/netpyoung/NF.Tool.ReleaseNoteMaker/actions/workflows/dotnet-test.yml)
[![Document](https://img.shields.io/badge/document-docfx-blue)](https://netpyoung.github.io/NF.Tool.ReleaseNoteMaker/)
[![License](https://img.shields.io/badge/license-MIT-C06524)](https://github.com/netpyoung/NF.Tool.ReleaseNoteMaker/blob/main/LICENSE.md)


wip

based on python [TownCrier](https://github.com/twisted/towncrier)

- [doc](https://netpyoung.github.io/NF.Tool.ReleaseNoteMaker/)

## used

- use [Toml format](https://toml.io/en/) and [xoofx/Tomlyn library](https://github.com/xoofx/Tomlyn) for Config file.
- use [T4 template](https://learn.microsoft.com/en-us/visualstudio/modeling/code-generation-and-t4-text-templates) and [mono/t4 library](https://github.com/mono/t4).
- use [Spectre.Console](https://spectreconsole.net/) for console output.
- use [Spectre.Console.Cli](https://spectreconsole.net/cli/) for parse args.
- use [SmartFormat](https://github.com/axuno/SmartFormat) for format string.