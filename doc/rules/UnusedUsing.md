<div class="project-logo">StyleChecker</div>
<div id="toc-level" data-values="H2,H3"></div>

# UnusedUsing

<div class="horizontal-scroll">

![UnusedUsing][fig-UnusedUsing]

</div>

## Summary

Unused `using` directives must be removed.

## Default severity

Warning

## Description

This rule reports diagnostics similar to CS8019 (unnecessary using directive).

Note that CS8019 reports diagnostics for auto-generated files,
but this analyzer doesn't.

## Code fix

StyleChecker does not provide a code fix; the Visual Studio IDE does.

## Example

### Diagnostic

```csharp
using System;

public class Main
{
    public void Method()
    {
    }
}
```

[fig-UnusedUsing]:
  https://maroontress.github.io/StyleChecker/images/UnusedUsing.png
