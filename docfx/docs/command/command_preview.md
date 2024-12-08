# Preview

- Preview a release note

``` sh
$ dotnet release-note preview --help
DESCRIPTION:
Preview a release note.

USAGE:
    dotnet release-note preview [OPTIONS]

OPTIONS:
                     DEFAULT
    -h, --help                  Prints help information
        --dir                   Build fragment in directory. Default to current directory
        --config                Pass a custom config file at FILE_PATH.
                                Default: ReleaseNote.config.toml
        --name                  Pass a custom project name
        --version    x.x.x      Render the news fragments using given version
        --date                  Render the news fragments using the given date
```