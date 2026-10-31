namespace StyleChecker.Analyzers.Cleaning.UnusedVariable;

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
using StyleChecker.Analyzers.Invocables;
using StyleChecker.Annotations;
using R = Resources;

/// <summary>
/// UnusedVariable analyzer.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Analyzer : AbstractAnalyzer
{
    /// <summary>
    /// The ID of this analyzer.
    /// </summary>
    public const string DiagnosticId = "UnusedVariable";

    private const string Category = Categories.Cleaning;
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

    private static void AnalyzeModel(
        SemanticModelAnalysisContext context)
    {
        var cancellationToken = context.CancellationToken;
        var model = context.SemanticModel;
        var root = model.SyntaxTree.GetCompilationUnitRoot(
            cancellationToken);
        CheckLocal(context, model);
        CheckParameter(context, model, root);
    }

    private static void CheckLocal(
        SemanticModelAnalysisContext context,
        SemanticModel model)
    {
        IEnumerable<ILocalSymbol> FindLocalSymbols(SyntaxToken token)
        {
            return model.LookupSymbols(token.Span.Start, null, token.ValueText)
                .OfType<ILocalSymbol>()
                .Take(1);
        }

        var all = LocalVariables.Symbols(model)
            .ToList();
        foreach (var (token, symbol) in all)
        {
            if (symbol.ContainingSymbol
                .DeclaringSyntaxReferences
                .FirstOrDefault() is not {} reference)
            {
                continue;
            }
            if (reference.GetSyntax()
                .DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Any(n => FindLocalSymbols(n.Identifier)
                    .Any(s => Symbols.AreEqual(s, symbol))))
            {
                continue;
            }
            var diagnostic = Diagnostic.Create(
                Rule,
                token.GetLocation(),
                R.TheLocalVariable,
                token,
                R.NeverUsed);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private static void CheckParameter(
        SemanticModelAnalysisContext context,
        SemanticModel model,
        CompilationUnitSyntax root)
    {
        var cancellationToken = context.CancellationToken;

        static bool IsEmptyBody(InvocableBaseNodePod pod)
        {
            return (pod.Body is null || !pod.Body.ChildNodes().Any())
                && pod.ExpressionBody is null;
        }

        static bool IsEmptyIndexer(IndexerDeclarationSyntax node)
        {
            if (node.ExpressionBody is not null)
            {
                return false;
            }
            if (node.AccessorList is not {} list)
            {
                return true;
            }
            return list.Accessors.All(a =>
                (a.Body is null || !a.Body.ChildNodes().Any())
                && a.ExpressionBody is null);
        }

        static bool IsMarkedAsUnused(AttributeData d)
        {
            var clazz = d.AttributeClass;
            return clazz is not null
                && clazz.ToString() == typeof(UnusedAttribute).FullName;
        }

        static bool ShouldParametersBeUsed(
            (InvocableBaseNodePod Pod, IMethodSymbol Symbol) i)
        {
            var (p, m) = i;
            return !m.IsAbstract
                && !((m.IsExtern
                        || m.IsVirtual
                        || p.Modifiers.Any(o => o.Text is "partial"))
                    && IsEmptyBody(p));
        }

        static bool ShouldIndexerParametersBeUsed(
            (IndexerDeclarationSyntax Node, IPropertySymbol Symbol) i)
        {
            var (node, symbol) = i;
            return !symbol.IsAbstract
                && symbol.ContainingType.TypeKind is not TypeKind.Interface
                && !((symbol.IsExtern || symbol.IsVirtual) && IsEmptyIndexer(node));
        }

        static bool IsParameterMarkedAsUnused(IParameterSymbol p)
        {
            return p.GetAttributes()
                .Any(IsMarkedAsUnused);
        }

        static bool IsMatchingParameter(ISymbol s, IParameterSymbol p)
        {
            return Symbols.AreEqual(s, p)
                || (s is IParameterSymbol ps
                    && ps.Locations.Length > 0
                    && p.Locations.Length > 0
                    && ps.Locations[0].Equals(p.Locations[0]));
        }

        void Report(IParameterSymbol p, string m)
        {
            var reference = p.DeclaringSyntaxReferences
                .FirstOrDefault();
            if (reference is null
                || reference.GetSyntax() is not ParameterSyntax node)
            {
                return;
            }
            var token = node.Identifier;
            var location = p.Locations[0];
            var diagnostic = Diagnostic.Create(
                Rule, location, R.TheParameter, token, m);
            context.ReportDiagnostic(diagnostic);
        }

        void ReportIfUnnecessarilyMarked(IEnumerable<IParameterSymbol> parameters)
        {
            var all = parameters
                .Where(IsParameterMarkedAsUnused);
            foreach (var p in all)
            {
                Report(p, R.MarkIsUnnecessary);
            }
        }

        Action ToAction(
            IEnumerable<IParameterSymbol> g,
            string m,
            Func<bool, bool> b)
        {
            var all = g.Where(p => b(IsParameterMarkedAsUnused(p)));
            return () =>
            {
                foreach (var q in all)
                {
                    Report(q, m);
                }
            };
        }

        void ReportIfUnusedOrMismarked(
            IEnumerable<SyntaxNode> scopeNodes,
            IEnumerable<IParameterSymbol> parameters)
        {
            var identifierNames = scopeNodes
                .SelectMany(n => n.DescendantNodes())
                .OfType<IdentifierNameSyntax>();
            var all = parameters
                .GroupBy(
                    p => identifierNames.Any(
                        n => FindSymbols(model, n.Identifier)
                            .Any(s => IsMatchingParameter(s, p))),
                    (isUsed, group) => isUsed
                        ? ToAction(group, R.UsedButMarkedAsUnused, b => b)
                        : ToAction(group, R.NeverUsed, b => !b));
            foreach (var action in all)
            {
                action();
            }
        }

        (InvocableBaseNodePod Pod, IMethodSymbol Symbol)?
            ToInvocation(InvocableBaseNodePod p)
        {
            var s = model.GetDeclaredSymbol(p.Node, cancellationToken);
            return (s is IMethodSymbol m)
                ? (p, m)
                : null;
        }

        var methods = root.DescendantNodes()
            .OfType<BaseMethodDeclarationSyntax>();
        var localFunctions = root.DescendantNodes()
            .OfType<LocalFunctionStatementSyntax>();
        var invocations = Enumerable.Empty<SyntaxNode>()
            .Concat(methods)
            .Concat(localFunctions)
            .Select(InvocableBaseNodePod.Of)
            .FilterNonNullReference()
            .Select(ToInvocation)
            .FilterNonNullValue()
            .ToList();
        foreach (var (pod, symbol) in invocations)
        {
            if (ShouldParametersBeUsed((pod, symbol)))
            {
                ReportIfUnusedOrMismarked([pod.Node], symbol.Parameters);
            }
            else
            {
                ReportIfUnnecessarilyMarked(symbol.Parameters);
            }
        }

        var indexers = root.DescendantNodes()
            .OfType<IndexerDeclarationSyntax>()
            .Select(node => (Node: node, Symbol: model.GetDeclaredSymbol(node, cancellationToken)))
            .Where(i => i.Symbol is not null)
            .Select(i => (i.Node, Symbol: i.Symbol!))
            .ToList();
        foreach (var (node, symbol) in indexers)
        {
            if (ShouldIndexerParametersBeUsed((node, symbol)))
            {
                ReportIfUnusedOrMismarked([node], symbol.Parameters);
            }
            else
            {
                ReportIfUnnecessarilyMarked(symbol.Parameters);
            }
        }

        var typeDeclarations = root.DescendantNodes()
            .OfType<TypeDeclarationSyntax>()
            .Where(t => t.ParameterList is not null);
        foreach (var typeDecl in typeDeclarations)
        {
            if (model.GetDeclaredSymbol(typeDecl, cancellationToken) is not {} typeSymbol
                || typeSymbol.IsRecord)
            {
                continue;
            }
            var primaryCtor = typeSymbol.Constructors
                .FirstOrDefault(c => c.DeclaringSyntaxReferences
                    .Any(r => r.GetSyntax() == typeDecl));
            if (primaryCtor is null)
            {
                continue;
            }
            ReportIfUnusedOrMismarked([typeDecl], primaryCtor.Parameters);
        }
    }

    private static IEnumerable<ISymbol> FindSymbols(
        SemanticModel model, SyntaxToken token)
    {
        return model.LookupSymbols(token.Span.Start, null, token.ValueText)
            .OfType<ISymbol>()
            .Take(1);
    }
}
