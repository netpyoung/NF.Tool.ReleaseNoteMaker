# Concept

## Flow

``` mermaid
flowchart LR

subgraph "ChangeLog.d/"
    Files["`
        1.added.md
        2.fixed.md
        3.security.md
        ...
    `"]
end

subgraph ReleaseNoteMaker
    Files --> Fragments
    Fragments -->|Arrange| Model(Model)
    Model --> |Render| Renderer
end

Renderer -->|t4    | Output
Renderer -->|liquid| Output


Renderer{Renderer}
Output[CHANGELOG.md]

style Files text-align:left
```
