<div class="project-logo">StyleChecker</div>
<div id="toc-level" data-values="H2,H3"></div>

# NoUsingDeclaration

<div class="horizontal-scroll">

![NoUsingDeclaration][fig-NoUsingDeclaration]

</div>

## Summary

Use using declarations to declare a local variable whenever possible.

## Default severity

Warning

## Description

When you declare a local variable and initialize it with an
[`IDisposable`][system.idisposable] object, you should use the using
declaration \[[1](#ref1)\] whenever possible as follows:

```csharp
// For implicitly-typed variables
using var reader = new StreamReader("file.txt");

// For explicitly-typed variables
using StreamReader i = new("file.txt");

// Multiple declarators are allowed in explicit type declarations
using StreamReader j = new("1.txt"), k = new("2.txt");
```

The using declaration is preferred to the using statement or the
`try`-`finally` idiom because it makes RAII easier to write in C#: the object
is disposed of automatically when its scope ends.

### Remarks

This rule reports a diagnostic only when the local variable is declared and
initialized with the `new` operator. It ignores a local variable declaration if
a method or property returns the `IDisposable` instance that initializes it.
For example:

```csharp
// OK
var output = System.Console.Out;

static TextReader NewStreamReader() => new StreamReader("file.txt");

// OK
var reader = NewStreamReader();
```

Initialization with an `IDisposable` instance created with an operator other
than the `new` operator is also not covered as follows:

```csharp
// OK (but you should prevent resource leaks)
TextReader reader = (inputFile is null)
    ? new StringReader(defaultText)
    : new StreamReader(inputFile);
```

Note that this example requires C# 9 or later (target-typed conditional
expression).

### Cases where no diagnostic is reported

This rule does not report a diagnostic when a factory method creates and
returns an `IDisposable` object as follows:

```csharp
public Socket? NewTcpSocket(Uri uri, int port)
{
    // XXX (you cannot use a using declaration)
    var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
    try
    {
        socket.Connect(uri.Host, port);
        return socket;
    }
    catch (Exception)
    {
        socket.Dispose();
    }
    return null;
}
```

Similarly, this rule does not report a diagnostic if you instantiate an
`IDisposable` object and assign it to a field or property or capture it as
follows:

```csharp
private StreamReader? streamReader;

private StreamWriter? SharedWriter { get; set; }

public void PrepareStream()
{
    // XXX (you cannot use a using declaration)
    var reader = new StreamReader("input.txt");
    streamReader = reader;
    var writer = new StreamWriter("output.txt");
    SharedWriter = writer;
    ⋮
}

public static Action WriteHelloAction()
{
    // XXX (you cannot use a using declaration)
    var writer = new StreamWriter("file.txt");
    return () =>
    {
        // How do you dispose of it!?
        writer.WriteLine("hello");
    };
}
```

This rule does not report a diagnostic in the following complex case, either,
where you instantiate an `IDisposable` object wrapped in a decorator pattern:

```csharp
public BufferedStream NewClientStream(…)
{
    // XXX (you cannot use a using declaration)
    var clientSocket = new Socket(
        AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    clientSocket.Connect(…);
    var netStream = new NetworkStream(clientSocket, true);
    var bufStream = new BufferedStream(netStream, streamBufferSize);
    return bufStream;
}
```

It also does not report a diagnostic when you reassign any value to the
variable as follows:

```csharp
// XXX (a using declaration causes an error CS1656 at the line /*💀*/.)
var i = new StreamReader("file.txt");
Console.WriteLine(i.ReadLine());
/*💀*/ i = new StreamReader("another.txt");
Console.WriteLine(i.ReadLine());
```

In summary, this rule does not report a diagnostic if the variable representing
the created instance is:

- used as an argument to a method or constructor
- on the right side of the assignment expression
- captured
- returned
- reassigned
- used as an element of a tuple
- used as an element of a collection expression

Therefore, you should be aware that resource leaks can occur even though this
rule does not report any diagnostics.

### Classes that do not need to be disposed

These types implement `IDisposable`, but their `Dispose()` method releases no
unmanaged resources. The local variables whose type is one of the following are
not covered:

- [`System.IO.StringReader`][system.io.stringreader]
- [`System.IO.StringWriter`][system.io.stringwriter]
- [`System.IO.MemoryStream`][system.io.memorystream]
- [`System.IO.UnmanagedMemoryStream`][system.io.unmanagedmemorystream]
- [`System.IO.UnmanagedMemoryAccessor`][system.io.unmanagedmemoryaccessor]

> See also [UnnecessaryUsing][] analyzer.

For example:

```csharp
// OK
var reader = new StringReader("hello");

// OK
StringReader i = new("hello");
```

However, even if the concrete class of the instance is one of those above, it is
covered if the type of the variable is not so, as follows:

```csharp
// NG
TextReader i = new StringReader("hello");
```

## Code fix

The code fix provides an option to add a `using` keyword before `var` or the
type name.

## Example

### Diagnostic

```csharp
public sealed class Code
{
    public static void Main()
    {
        var i = new StreamReader("file.txt");
        StreamReader j = new("file.txt");
    }
}
```

### Code fix

```csharp
public sealed class Code
{
    public static void Main()
    {
        using var i = new StreamReader("file.txt");
        using StreamReader j = new("file.txt");
    }
}
```

## References

<a id="ref1"></a> [1]
[Microsoft, _"pattern-based using" and "using declarations"_][dotnet-csharp-using-declaration]

[dotnet-csharp-using-declaration]:
  https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-8.0/using
[system.io.memorystream]:
  https://docs.microsoft.com/en-us/dotnet/api/system.io.memorystream?view=netstandard-1.0
[system.io.unmanagedmemorystream]:
  https://docs.microsoft.com/en-us/dotnet/api/system.io.unmanagedmemorystream?view=netstandard-2.0
[system.io.unmanagedmemoryaccessor]:
  https://docs.microsoft.com/en-us/dotnet/api/system.io.unmanagedmemoryaccessor?view=netstandard-2.0
[system.io.stringreader]:
  https://docs.microsoft.com/en-us/dotnet/api/system.io.stringreader?view=netstandard-1.0
[system.io.stringwriter]:
  https://docs.microsoft.com/en-us/dotnet/api/system.io.stringwriter?view=netstandard-1.0
[system.idisposable]:
  https://docs.microsoft.com/en-us/dotnet/api/system.idisposable?view=netstandard-1.0
[fig-NoUsingDeclaration]:
  https://maroontress.github.io/StyleChecker/images/NoUsingDeclaration.png
[UnnecessaryUsing]: UnnecessaryUsing.md
