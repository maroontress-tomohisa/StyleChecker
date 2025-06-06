<div class="project-logo">StyleChecker</div>
<div id="toc-level" data-values="H2,H3"></div>

# NoSingleSpaceAfterTripleSlash

<div class="horizontal-scroll">

![NoSingleSpaceAfterTripleSlash][fig-NoSingleSpaceAfterTripleSlash]

</div>

## Summary

Triple slash `///` (Single Line Documentation Comment) should be followed
by a single space.

## Default severity

Warning

## Description

This is a replacement for [SA1004][sa1004].

StyleCop.Analyzers (1.1.118) emits SA1004 to the following code:

```csharp
/// <seealso cref="LineBreakInsideAttribute
/// (string, string)"/>
/// <seealso cref="LineBreakInsideAttribute(
/// string, string)"/>
/// <seealso cref="LineBreakInsideAttribute(string,
/// string)"/>
/// <seealso cref="LineBreakInsideAttribute(string, string
/// )"/>
public void LineBreakInsideAttribute(string a, string b)
{
}
```

This analyzer does not report diagnostics to the code, which includes
a line break inside the start/end tags of an XML element,
as long as a single space follows `///`.

## Remarks

It is not sufficient to write a `///` followed by a single whitespace character.
More precisely, `///` must be followed by a single whitespace character followed
by an XML Element (`<element>…</element>` or `<element … />`). If the XML
element is long, it can be wrapped, but each wrapped line must begin with `///`
followed by one or more whitespace characters. The following is a conformance
example:

```csharp
/// <summary>Hello.</summary>
/// <seealso cref="Good"/>
public static void Good()
{
}

/// <summary>Hello
/// World.</summary>
public static void GoodWithWrapping()
{
}
```

The following is a nonconformance example:

```csharp
/// Hello <summary>World.</summary>
/// See <seealso cref="Bad"/>
public static void Bad()
{
}

/// Hello.
public static void BadWithNoElements()
{
}
```

Before version 2.0.1, this analyzer issued diagnostics for `Hello` and `See` in
`Bad()`. The warning was &ldquo;A single white space is needed after
'///',&rdquo; but a more appropriate warning should have been &ldquo;Only one
whitespace character should be placed between /// and the XML element.&rdquo;

Even after version 2.0.1, [StrayText][] analyzer reports `Bad()` and
`BadWithNoElements()` instead of this analyzer.

## Code fix

The code fix provides an option inserting a single space after `///`,
or replacing two or more spaces after `///` with a single space.
Note that Code Fix provider keeps two or more spaces in the text content
of an XML element, as well as SA1004.

## Example

### Diagnostic

```csharp
///<summary>
///  summary with extra indent.
///</summary>
///  <param name="a">first parameter.</param>
///   <param name="b">second parameter.</param>
/// <remarks>
///remarks.
/// </remarks>
public void Method(int a, int b)
{
}
```

### Code fix

```csharp
/// <summary>
///  summary with extra indent.
/// </summary>
/// <param name="a">first parameter.</param>
/// <param name="b">second parameter.</param>
/// <remarks>
/// remarks.
/// </remarks>
public void Method(int a, int b)
{
}
```

[sa1004]:
  https://github.com/DotNetAnalyzers/StyleCopAnalyzers/blob/master/documentation/SA1004.md
[fig-NoSingleSpaceAfterTripleSlash]:
  https://maroontress.github.io/StyleChecker/images/NoSingleSpaceAfterTripleSlash.png
[StrayText]: StrayText.md
