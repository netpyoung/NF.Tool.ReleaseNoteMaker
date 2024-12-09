# Concept

news fragments/newsfiles/topfiles

## Example

``` txt
USAGE:
    dotnet release-note [OPTIONS] <COMMAND>

EXAMPLES:
    dotnet release-note init
    dotnet release-note create --edit
    dotnet release-note create 1.added.md --content "Hello World"
    dotnet release-note build --version 1.0.0
    dotnet release-note check

OPTIONS:
    -h, --help    Prints help information

COMMANDS:
    init       Init release-note setup
    create     Create a new fragment
    preview    Preview a release note
    build      Build a release note
    check      Checks files changed
```