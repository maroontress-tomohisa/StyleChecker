<div class="project-logo">StyleChecker</div>
<div id="toc-level" data-values="H2,H3"></div>

# PostIncrement

<div class="horizontal-scroll">

![PostIncrement][fig-PostIncrement]

</div>

## Summary

Avoid post-increment/decrement operators (e.g. `i++`, `i--`),
if they can be replaced with pre-increment/decrement ones
(e.g. `++i`, `--i`).

## Default severity

Warning

## Description

As a general style guideline, not as a syntactic requirement, unary operators
must be followed by their operands. If the post-increment/decrement operator
doesn't make sense in evaluating the expression, but only its side effects are
needed, it must be replaced with the pre-increment/decrement one.

## Code fix

The code fix provides an option to replace the post-increment/decrement
operator with the pre-increment/decrement one.

## Example

### Diagnostic

```csharp
i++;
```

### Code fix

```csharp
++i;
```

[fig-PostIncrement]:
  https://maroontress.github.io/StyleChecker/images/PostIncrement.png
