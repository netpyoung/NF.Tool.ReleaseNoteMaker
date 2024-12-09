# Tutorial

``` sh
$ dotnet release-note init
Initialized
./
├── ReleaseNote.config.toml
└── ChangeLog.d/
    └── Template.tt
$ dotnet release-note preview
# x.x.x (2024-12-09)



$ dotnet release-note create 123.feature.md
Created news fragment at 123.feature.md

$ dotnet release-note preview
# x.x.x (2024-12-09)


## Main

### Features

- Add your info here (#123)



$ dotnet release-note create
Issue Name :
ex) + / +hello / 123 / baz.1.2
Default: (+): 456
Created news fragment at C:\Users\pyoung\temp2\ChangeLog.d\456.changed.md

$ dotnet release-note preview
# x.x.x (2024-12-09)


## Main

### Changed

- Add your info here (#456)

### Features

- Add your info here (#123)



$ dotnet release-note build --version 1.0.0
* Finding news fragments...
* Loading template...
* Rendering news fragments...
* Writing to newsfile...
    ❗ C:/Users/pyoung/temp2/CHANGELOG.md
* Staging newsfile...
    ➕ C:\Users\pyoung\temp2\CHANGELOG.md
I want to remove the following files:
    ❌ C:\Users\pyoung\temp2\ChangeLog.d\123.feature.md
    ❌ C:\Users\pyoung\temp2\ChangeLog.d\456.changed.md
Is it okay if I remove those files? [y/n] (y): y
* Removing news fragments...
* Done!
$
$ cat .\CHANGELOG.md
# 1.0.0 (2024-12-09)


## Main

### Changed

- Add your info here (#456)

### Features

- Add your info here (#123)
```

dotnet release-note create --content "Added a cool feature!"          1.added.md
dotnet release-note create --content "Changed a behavior!"            2.changed.md
dotnet release-note create --content "Deprecated a module!"           3.deprecated.md
dotnet release-note create --content "Removed a square feature!"      4.removed.md
dotnet release-note create --content "Fixed a bug!"                   5.fixed.md
dotnet release-note create --content "Fixed a security issue!"        6.security.md
dotnet release-note create --content "Fixed a security issue!"        7.security.md
dotnet release-note create --content "A fix without an issue number!" +something-unique.fixed.md