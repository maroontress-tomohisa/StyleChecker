namespace StyleChecker.Analyzers.Refactoring.StaticGenericClass;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Maroontress.Roastery;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using StyleChecker.Analyzers;
using R = Resources;

/// <summary>
/// StaticGenericClass analyzer.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Analyzer : AbstractAnalyzer
{
    /// <summary>
    /// The ID of this analyzer.
    /// </summary>
    public const string DiagnosticId = "StaticGenericClass";

    private const string Category = Categories.Refactoring;
    private static readonly DiagnosticDescriptor Rule = NewRule();

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor>
        SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    private protected override void Register(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.RegisterSemanticModelAction(AnalyzeModel);
    }

    private static DiagnosticDescriptor NewRule()
    {
        var localize = Localizers.Of<R>(R.ResourceManager);
        return new DiagnosticDescriptor(
            DiagnosticId,
            localize(nameof(R.Title)),
            localize(nameof(R.MessageFormat)),
            Category,
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: localize(nameof(R.Description)),
            helpLinkUri: HelpLink.ToUri(DiagnosticId));
    }

    private static bool HasNoUnsafeMethod(
            ClassDeclarationSyntax node,
            Func<MethodDeclarationSyntax, bool> isMethodSafe)
    {
        /*
            If there are any methods whose parameters do not include all of
            the class's type parameters, it is not safe to refactor the class.
            For example:

                public static class C1<T>
                    where T : class
                {
                    public static void M1()
                    {
                    }

                    public static T? M2() => default;
                }

                public static class C2<T, U>
                    where T : class
                    where U : class
                {
                    public static void M3(T instance)
                    {
                    }
                }

            Also, if any method declares a type parameter with the same name
            as one of the class's type parameters, it is not safe to refactor
            the class. For example:

                public static class C3<T>
                    where T : class
                {
                    public static void M4<T>(T instance)
                    {
                    }
                }

            Also, if any method declares its own type parameter that is not
            used in its parameter types, it is not safe to refactor the class.
            For example:

                public static class C4<T>
                    where T : class
                {
                    public static U? M5<U>(T instance) => default;
                }
        */
        var methods = node.Members
            .OfType<MethodDeclarationSyntax>();
        return methods.Any()
            && methods.All(isMethodSafe);
    }

    private static bool AreMembersSafe(
        ClassDeclarationSyntax node,
        IEnumerable<TypeParameterSyntax> typeParameters,
        ISymbol symbol,
        ISymbolizer symbolizer)
    {
        var classTypeParameterSet = typeParameters.Select(symbolizer.ToSymbol)
            .FilterNonNullReference()
            .ToImmutableHashSet(SymbolEqualityComparer.Default);

        bool IsClassTypeParameter(ISymbol s)
            => Symbols.AreEqual(s.ContainingSymbol, symbol);

        bool ContainsClassTypeParameter(IEnumerable<SyntaxNode> nodes)
            => nodes.Where(n => n.IsKind(SyntaxKind.IdentifierName))
                .Select(n => symbolizer.ToSymbolInfo(n).Symbol)
                .FilterNonNullReference()
                .Where(s => s.Kind is SymbolKind.TypeParameter)
                .Any(IsClassTypeParameter);

        bool HasUnsafeAttribute()
            => ContainsClassTypeParameter(
                node.AttributeLists
                    .SelectMany(a => a.DescendantNodesAndSelf()));

        bool HasNoUnsafeNonMethodMember()
        {
            /*
                If the class has any non-method member other than a constant
                field of a value type or string that does not reference the
                class's type parameters, it is not safe to refactor the class.
                For example:

                    public static class C<T> where T : class
                    {
                        public static T? Instance { get; } = default;
                        public const string TypeName = nameof(T);

                        public static void M(T instance) {}
                    }
            */
            return node.Members
                .Where(m => m is not MethodDeclarationSyntax)
                .All(m => m is FieldDeclarationSyntax field
                    && field.Modifiers.Any(SyntaxKind.ConstKeyword)
                    && !ContainsClassTypeParameter(field.DescendantNodes()));
        }

        ImmutableHashSet<ISymbol> NewParameterTypeSymbolSet(
            SeparatedSyntaxList<ParameterSyntax> parameters)
        {
            return parameters.SelectMany(p => p.Type is {} parameterType
                    ? parameterType.DescendantNodesAndSelf()
                    : [])
                .Where(n => n.IsKind(SyntaxKind.IdentifierName))
                .Select(n => symbolizer.ToSymbolInfo(n).Symbol)
                .FilterNonNullReference()
                .Where(s => s.Kind is SymbolKind.TypeParameter)
                .ToImmutableHashSet(SymbolEqualityComparer.Default);
        }

        bool IsMethodSafe(MethodDeclarationSyntax m)
        {
            /*
                Return false if:

                1. The method has no parameters.
                2. The method has parameters, but their types contain no type
                   parameters.
                3. At least one type parameter of the CLASS is missing from
                   the method's parameter types.
                4. The METHOD has type parameters, and at least one of them is
                   missing from the method's parameter types.

                Note that the following code corresponds to 3:

                    public static class C<T> where T : class
                    {
                        public static void M<T>(T instance) {}
                    }
            */
            return
                /* 1 */
                m.ParameterList.Parameters is { Count: > 0 } methodParameters
                /* 2 */
                && NewParameterTypeSymbolSet(methodParameters)
                    is { Count: > 0 } typeParametersInParameters
                /* 3 */
                && classTypeParameterSet.IsSubsetOf(typeParametersInParameters)
                /* 4 */
                && (m.TypeParameterList
                        is not { Parameters: { Count: > 0 } typeParameters }
                    || typeParameters.Select(symbolizer.ToSymbol)
                        .FilterNonNullReference()
                        .All(s => typeParametersInParameters.Contains(s)));
        }

        return !HasUnsafeAttribute()
            && HasNoUnsafeNonMethodMember()
            && HasNoUnsafeMethod(node, IsMethodSafe);
    }

    private static Func<ClassDeclarationSyntax, bool> NewIsTarget(
        ISymbolizer symbolizer)
    {
        return node => symbolizer.ToSymbol(node) is {} symbol
            && symbol.IsStatic
            && !node.Modifiers.Any(SyntaxKind.PartialKeyword)
            && node.TypeParameterList
                is { Parameters: { Count: > 0 } parameters }
            && node.Members.Count > 0
            && AreMembersSafe(node, parameters, symbol, symbolizer);
    }

    private static void AnalyzeModel(SemanticModelAnalysisContext context)
    {
        var isTarget = NewIsTarget(context.GetSymbolizer());
        var root = context.GetCompilationUnitRoot();
        var all = root.DescendantNodes()
            .OfType<ClassDeclarationSyntax>()
            .Where(isTarget);
        foreach (var node in all)
        {
            var id = node.Identifier;
            var location = id.GetLocation();
            var diagnostic = Diagnostic.Create(Rule, location, id.Text);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
