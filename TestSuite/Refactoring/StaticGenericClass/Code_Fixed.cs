#pragma warning disable CS0693

namespace StyleChecker.Test.Refactoring.StaticGenericClass;

using System;

public static class Code
{
    public static void Method<T>(T instance) where T : class
    {
    }
}

public static class NoConstraintClause
{
    public static void Method<T>(T instance)
    {
    }
}

public static class MultipleTypeParameter
{
    public static void Method<T, U>(T instance, U option) where T : class
        where U : class
    {
    }
}

/// <summary>Class Summary.</summary>
/// 
public static class SingleLineDocumentationComment
{
    /// <summary>Method summary.</summary>
    /// <param name="instance">Parameter.</param>
    /// <typeparam name="T">Class type parameter.</typeparam>
    public static void Method<T>(T instance) where T : class
    {
    }

    /**
     * <summary>Method Summary.</summary>
     * <param name="instance">Parameter.</param>
     * <typeparam name="T">Class type parameter.</typeparam>
     */
    public static void TheOtherStyleComment<T>(T instance) where T : class
    {
    }

    /// <typeparam name="T">Class type parameter.</typeparam>
    public static void NoDocumentationComment<T>(T instance) where T : class
    {
    }
}

/**
    <summary>Class Summary.</summary>

*/
public static class MultiLineDocumentationComment
{
    /**
        <summary>Method summary.</summary>
        <param name="instance">Parameter.</param>
        <typeparam name="T">Class type parameter.</typeparam>
    */
    public static void Method<T>(T instance) where T : class
    {
    }

    /// <summary>Method Summary.</summary>
    /// <param name="instance">Parameter.</param>
    /// <typeparam name="T">Class type parameter.</typeparam>
    public static void TheOtherStyleComment<T>(T instance) where T : class
    {
    }

    /// <typeparam name="T">Class type parameter.</typeparam>
    public static void NoDocumentationComment<T>(T instance) where T : class
    {
    }
}

public static class Reference
{
    public static void Caller()
    {
        Code.Method("hello");
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

[CustomAttribute("T")]
public static class WithSafeAttribute
{
    public static void Method<T>(T x)
    {
    }
}

public static class ConstantAndMethods
{
    public const int Max = 10;
    public const string Name = "ConstantAndMethods";
    public const object Nothing = null;

    public static void Method<T>(T value)
    {
    }
}

public static class UserOfConstantAndMethods
{
    public const int Max = ConstantAndMethods.Max;
    public const string Name = ConstantAndMethods.Name;
}
