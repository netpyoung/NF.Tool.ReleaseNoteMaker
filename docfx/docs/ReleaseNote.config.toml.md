# ReleaseNote.config.toml

TODO(pyoung)

## sample

``` toml
[ReleaseNote.Maker]
Directory = "ChangeLog.d"
OutputFileName = "CHANGELOG.md"
TemplateFilePath = "ChangeLog.d/Template.tt"
EndOfLine = "ENVIRONMENT"

[[ReleaseNote.Section]]
Path = ""
DisplayName = "Main"

[[ReleaseNote.Type]]
Category = "added"
DisplayName = "Added"
IsShowContent = true
```

- [source](https://github.com/netpyoung/NF.Tool.ReleaseNoteMaker/blob/main/NF.Tool.ReleaseNoteMaker/NF.Tool.ReleaseNoteMaker.Common/Config/ReleaseNoteConfig.cs)

##

### \[ReleaseNote.Maker]

Directory = "ChangeLog.d"
OutputFileName = "CHANGELOG.md"
TemplateFilePath = "ChangeLog.d/Template.tt"
EndOfLine = "ENVIRONMENT"


### \[[ReleaseNote.Section]]

Path = ""
DisplayName = "Main"


### \[[ReleaseNote.Type]]

Category = "feature"
DisplayName = "Features"
IsShowContent = true
