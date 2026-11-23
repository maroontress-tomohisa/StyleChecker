#nullable enable
#pragma warning disable CS0693

namespace StyleChecker.Test.Refactoring.StaticGenericClass;

using System;

public static class Empty<T>
{
}

public static class NameClash<T>
{
    public static void Method<T>(T instance)
    {
    }
}

public static class MethodWhoseParametersDoesNotContainT<T>
{
    public static void Method<U>(U instance)
    {
    }
}

public static class ReturnTypeOnly<T>
{
    public static T? Create() => default;
    public static T? Create(string label) => default;
}

public static class WithField<T>
{
    public static T? Default = default;

    public static void Method(T instance)
    {
    }
}

public static class WithProperty<T>
{
    public static T? Default { get; } = default;

    public static void Method(T instance)
    {
    }
}

public static class NotAllMethodsWithT<T>
{
    public static void Method(T instance)
    {
    }

    public static void Other()
    {
    }
}

public static class PartiallyUsed<T, U>
{
    public static void Method(T instance)
    {
    }
}

public static partial class Partial<T>
{
    public static void Method(T instance)
    {
    }
}

public static class ParameterNamedT<T>
{
    public static void Method(int T)
    {
    }
}

public static class NoMethod<T>
{
    public static int Value => 1;
}

public static class Holder
{
    public sealed class T
    {
    }
}

public static class QualifiedName<T>
{
    public static void Method(Holder.T instance)
    {
    }
}

public static class HasMutableState<T>
{
    public static int CallCount
    {
        get;
        private set;
    }

    public static void Method(T value) => ++CallCount;
}

public static class HasAnotherTypeParameterNotInTheParameters<T>
{
    public static U Convert<U>(T value) where U : new() => new U();
}

public static class HasAnotherTypeParameterThatIsNoParameterTypes<T>
{
    public static void M<U>(T x, string name = nameof(U))
    {
    }
}

[AttributeUsage(
    AttributeTargets.Class,
    Inherited = false,
    AllowMultiple = false)]
public sealed class CustomAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}

[CustomAttribute(nameof(T))]
public static class WithUnsafeAttribute<T>
{
    public static void Method(T x)
    {
    }
}

public static class WithUnsafeConstant<T>
{
    public const bool IsT = nameof(T) == "T";
    public static void Method(T value)
    {
    }
}
