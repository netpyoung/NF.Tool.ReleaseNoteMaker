# Introduction

- [repo](https://github.com/netpyoung/NF.Tool.ReleaseNoteMaker/)

## Example

``` txt
USAGE:
    release-note-maker [OPTIONS] <COMMAND>

EXAMPLES:
    release-note-maker init
    release-note-maker create --edit
    release-note-maker create 1.added.md --content "Hello World"
    release-note-maker build --version 1.0.0
    release-note-maker check

OPTIONS:
    -h, --help    Prints help information

COMMANDS:
    init       Create a new config file
    create     Create a new fragment
    preview    Preview a release note
    build      Generate a release note
    check      Checks files changed
```