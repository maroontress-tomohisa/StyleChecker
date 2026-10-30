<div class="project-logo">StyleChecker</div>
<div id="toc-level" data-values="H2,H3"></div>

# StaticGenericClass

<div class="horizontal-scroll">

![StaticGenericClass][fig-StaticGenericClass]

</div>

## Summary

Move type parameters from the static class to its methods if possible.

## Default severity

Warning

## Description

Replace `Class<T>.Method()` with `Class.Method<T>()` if `T` appears in the
method's parameter types, so that the compiler can infer `T` from the
arguments.

## Code fix

The code fix provides an option to move the type parameters from the static
class to its methods.

## Example

### Diagnostic

```csharp
/// <summary>Class Summary.</summary>
/// <typeparam name="T">Type parameter.</typeparam>
public static class Code<T> where T : class
{
    /// <summary>Method summary.</summary>
    /// <param name="instance">Parameter.</param>
    public static void Method(T instance)
    {
        ⋮
    }
}

public class AnotherClass
{
    public void AnotherMethod()
    {
        Code<string>.Method("...");
    }
}
```

### Code fix

```csharp
/// <summary>Class Summary.</summary>
public static class Code
{
    /// <summary>Method summary.</summary>
    /// <param name="instance">Parameter.</param>
    /// <typeparam name="T">Type parameter.</typeparam>
    public static void Method<T>(T instance) where T : class
    {
        ⋮
    }
}

public class AnotherClass
{
    public void AnotherMethod()
    {
        Code.Method("...");
    }
}
```

[fig-StaticGenericClass]:
  https://maroontress.github.io/StyleChecker/images/StaticGenericClass.png
