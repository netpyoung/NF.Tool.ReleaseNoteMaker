# NF.Tool.ReleaseNoteMaker

[![GitHub](https://img.shields.io/badge/GitHub-%23121011.svg?logo=github&logoColor=white)](https://github.com/netpyoung/NF.Tool.ReleaseNoteMaker)
[![.NET Test Workflow](https://github.com/netpyoung/NF.Tool.ReleaseNoteMaker/actions/workflows/dotnet-test.yml/badge.svg)](https://github.com/netpyoung/NF.Tool.ReleaseNoteMaker/actions/workflows/dotnet-test.yml)
[![Document](https://img.shields.io/badge/document-docfx-blue)](https://netpyoung.github.io/NF.Tool.ReleaseNoteMaker/)
[![License](https://img.shields.io/badge/license-MIT-C06524)](https://github.com/netpyoung/NF.Tool.ReleaseNoteMaker/blob/main/LICENSE.md)
[![NuGet](https://img.shields.io/nuget/v/dotnet-release-note.svg?style=flat&label=NuGet%3A%20dotnet-release-note)](https://www.nuget.org/packages/dotnet-release-note/)

- Reinvent the wheel

## Overview

`NF.Tool.ReleaseNoteMaker (akka. dotnet-release-note)` simplifies changelog creation in .NET projects by mimicking the functionality of Python's [twisted/towncrier](https://github.com/twisted/towncrier).
It enables developers to manage changelog entries incrementally and consolidate them during release.

## Install

``` bash
dotnet tool install --global dotnet-release-note
```

## Document

- [Documentation](https://netpyoung.github.io/NF.Tool.ReleaseNoteMaker/docs/concept.html)

## Dependencies

- use [Toml format](https://toml.io/en/) and [xoofx/Tomlyn library](https://github.com/xoofx/Tomlyn) for Config file.
- use [T4 template](https://learn.microsoft.com/en-us/visualstudio/modeling/code-generation-and-t4-text-templates) and [mono/t4 library](https://github.com/mono/t4).
- use [Spectre.Console](https://spectreconsole.net/) for console output.
- use [Spectre.Console.Cli](https://spectreconsole.net/cli/) for parse args.
- use [SmartFormat](https://github.com/axuno/SmartFormat) for format string.
- use [sebastienros/fluid](https://github.com/sebastienros/fluid) for [liquid](https://shopify.github.io/liquid/)

## License

This project is licensed under the MIT License. See the [LICENSE](https://github.com/netpyoung/NF.Tool.ReleaseNoteMaker/blob/main/LICENSE.md) file for details.

