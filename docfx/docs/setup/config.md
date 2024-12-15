# ReleaseNote.config.toml

## ReleaseNote.config.toml(default)

[!code-toml[Default](~/../NF.Tool.ReleaseNoteMaker/NF.Tool.ReleaseNoteMaker.CLI/res/ReleaseNote.config.toml)]

## Detail

### \[ReleaseNote.Maker]

| Key              | Default                           |
| ---------------- | --------------------------------- |
| Directory        | "ChangeLog.d"                     |
| OutputFileName   | "CHANGELOG.md"                    |
| TemplateFilePath | "ChangeLog.d/Template.tt"         |
| Name             | ""                                |
| Version          | ""                                |
| CsprojPath       | ""                                |
| Ignores          | []                                |
| OrphanPrefix     | "+"                               |
| IssuePattern     | ""                                |
| IssueFormat      | ""                                |
| TitleFormat      | ""                                |
| StartString      | "<!-- release notes start -->\n"; |
| IsWrap           | false                             |
| IsAllBullets     | false                             |
| IsSingleFile     | true                              |
| EndOfLine        | "LF"                              |

### \[ReleaseNote.Reader]

| Key            | Default |
| -------------- | ------- |
| VersionPattern |         |
| TitlePattern   |         |

### \[[ReleaseNote.Section]]

| Key         | Default |
| ----------- | ------- |
| Path        | ""      |
| DisplayName | "Main"  |


### \[[ReleaseNote.Type]]

| Key           | Default    |
| ------------- | ---------- |
| Category      | "feature"  |
| DisplayName   | "Features" |
| IsShowContent | true       |
