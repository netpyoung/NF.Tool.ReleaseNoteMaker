# Check

- Check files changed


``` sh
$ dotnet release-note check --help
DESCRIPTION:
Checks files changed.

USAGE:
    dotnet release-note check [OPTIONS]

EXAMPLES:
    dotnet release-note check

OPTIONS:
                          DEFAULT
    -h, --help                           Prints help information
        --dir                            Build fragment in directory. Default to current directory
        --config                         Pass a custom config file at FILE_PATH.
                                         Default: ReleaseNote.config.toml
        --compare-with    origin/main    Checks files changed running git diff --name-only BRANCH...
                                         BRANCH is the branch to be compared with
```