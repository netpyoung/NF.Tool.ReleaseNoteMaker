# Tutorial


0. Install

``` sh
$ dotnet tool install --global dotnet-release-note
```

``` sh
$ dotnet release-note
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

2. Init

``` sh
$ dotnet release-note init
Initialized
./
├── ReleaseNote.config.toml
├── CHANGELOG.md
└── ChangeLog.d/
    └── Template.t4
```

3. Create a changelog entry:
   
    Add a new file in the changelog/ directory, named with the issue or pull request number and type.

``` sh
dotnet release-note create --content "Added a cool feature!"          1.added.md
dotnet release-note create --content "Changed a behavior!"            2.changed.md
dotnet release-note create --content "Deprecated a module!"           3.deprecated.md
dotnet release-note create --content "Removed a square feature!"      4.removed.md
dotnet release-note create --content "Fixed a bug!"                   5.fixed.md
dotnet release-note create --content "Fixed a security issue!"        6.security.md
dotnet release-note create --content "Fixed a security issue!"        7.security.md
dotnet release-note create --content "A fix without an issue number!" +something-unique.fixed.md
```

``` sh
ChangeLog.d/
├── 1.added.md
├── 2.changed.md
├── 3.deprecated.md
├── 4.removed.md
├── 5.fixed.md
├── 6.security.md
├── 7.security.md
└── +something-unique.fixed.md
```

4. Preview changelog

``` sh
$ dotnet release-note preview
## PREVIEW (2025-11-20)


### Main

#### Added

- Added a cool feature! (#1)

#### Removed

- Removed a square feature! (#4)

#### Changed

- Changed a behavior! (#2)

#### Deprecated

- Deprecated a module! (#3)

#### Fixed

- Fixed a bug! (#5)
- A fix without an issue number!

#### Security

- Fixed a security issue! (#6, #7)



```

5. Build changelog

``` sh
$ dotnet release-note build --version 1.0.0
* Finding news fragments...
* Loading template...
* Rendering news fragments...
* Writing to newsfile...
    ❗ C:/Users/pyoung/temp2/CHANGELOG.md
* Staging newsfile...
    ➕ C:\Users\pyoung\temp2\CHANGELOG.md
I want to remove the following files:
    ❌ C:\Users\pyoung\temp2\ChangeLog.d\+something-unique.fixed.md
    ❌ C:\Users\pyoung\temp2\ChangeLog.d\1.added.md
    ❌ C:\Users\pyoung\temp2\ChangeLog.d\2.changed.md
    ❌ C:\Users\pyoung\temp2\ChangeLog.d\3.deprecated.md
    ❌ C:\Users\pyoung\temp2\ChangeLog.d\4.removed.md
    ❌ C:\Users\pyoung\temp2\ChangeLog.d\5.fixed.md
    ❌ C:\Users\pyoung\temp2\ChangeLog.d\6.security.md
    ❌ C:\Users\pyoung\temp2\ChangeLog.d\7.security.md
Is it okay if I remove those files? [y/n] (y): y
* Removing news fragments...
* Done!
```

```
$ cat .\CHANGELOG.md
# Change Log

<!-- release notes start -->

## 1.0.0 (2025-11-20)


### Main

#### Added

- Added a cool feature! (#1)

#### Removed

- Removed a square feature! (#4)

#### Changed

- Changed a behavior! (#2)

#### Deprecated

- Deprecated a module! (#3)

#### Fixed

- Fixed a bug! (#5)
- A fix without an issue number!

#### Security

- Fixed a security issue! (#6, #7)
```
