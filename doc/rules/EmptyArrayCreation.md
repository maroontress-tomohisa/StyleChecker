<div class="project-logo">StyleChecker</div>
<div id="toc-level" data-values="H2,H3"></div>

# EmptyArrayCreation

<div class="horizontal-scroll">

![EmptyArrayCreation][fig-EmptyArrayCreation]

</div>

## Summary

Use `System.Array.Empty<T>()` instead of an empty array creation.

## Default severity

Warning

## Description

An empty array is immutable, so it can be shared safely with all threads. And
creating empty arrays many times is inefficient because the repeated
allocations make your CPU spend more time on garbage collection. So the empty
array object must be created only once and be shared.

This rule covers only one-dimensional arrays, including jagged arrays such as
`int[][]`. Multidimensional arrays such as `int[,]` are not covered.

## Code fix

The code fix provides an option to replace the empty array creation with
`System.Array.Empty<T>()`.

## Example

### Diagnostic

```csharp
public string[] EmptyStringArray { get; }
    = new string[0];

public int[] Method()
{
    var emptyIntegers = new int[] {};
    ⋮
```

### Code fix

```csharp
public string[] EmptyStringArray { get; }
    = System.Array.Empty<string>();

public int[] Method()
{
    var emptyIntegers = System.Array.Empty<int>();
    ⋮
```

[fig-EmptyArrayCreation]:
  https://maroontress.github.io/StyleChecker/images/EmptyArrayCreation.png
