# ReleaseNote.config.toml

- [source](https://github.com/netpyoung/NF.Tool.ReleaseNoteMaker/blob/main/NF.Tool.ReleaseNoteMaker/NF.Tool.ReleaseNoteMaker.Common/Config/ReleaseNoteConfig.cs)

TODO(pyoung)

## ReleaseNote.config.toml(default)

[!code-toml[Default](~/../NF.Tool.ReleaseNoteMaker/NF.Tool.ReleaseNoteMaker.CLI/res/ReleaseNote.config.toml)]

## detail

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
