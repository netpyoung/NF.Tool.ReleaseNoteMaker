# Concept

## Flow

``` mermaid
flowchart LR


Files["`
    1.added.md
    2.fixed.md
    3.security.md
    ...
`"]

Renderer{Renderer}
Output[ChangeLog.md]

Files --> Fragments

subgraph ReleaseNoteMaker
    Fragments -->|Arragne| Model(Model)
    Model --> Renderer
end

Renderer -->|t4    | Output
Renderer -->|liquid| Output

style Files text-align:left
```
