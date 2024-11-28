# Introduction

- [repo](https://github.com/netpyoung/NF.Tool.ReleaseNoteMaker/)

## Example

``` txt
$ NF.Tool.ReleaseNoteMaker.CLI.exe --help
USAGE:
    NF.Tool.ReleaseNoteMaker.CLI.dll [OPTIONS] <COMMAND>

EXAMPLES:
    NF.Tool.ReleaseNoteMaker.CLI.dll init
    NF.Tool.ReleaseNoteMaker.CLI.dll init --file ReleaseNote.config.toml
    NF.Tool.ReleaseNoteMaker.CLI.dll create --edit
    NF.Tool.ReleaseNoteMaker.CLI.dll create --content "Hello World" 1.added.md
    NF.Tool.ReleaseNoteMaker.CLI.dll build --version 1.0.0

OPTIONS:
    -h, --help    Prints help information

COMMANDS:
    init      Create a new config file
    create    Create a new fragment
    build     Generate a release note
```