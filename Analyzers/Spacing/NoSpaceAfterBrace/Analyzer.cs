namespace StyleChecker.Analyzers.Spacing.NoSpaceAfterBrace;

using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using StyleChecker.Analyzers;
using R = Resources;

/// <summary>
/// NoSpaceAfterBrace analyzer.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Analyzer : AbstractAnalyzer
{
    /// <summary>
    /// The ID of this analyzer.
    /// </summary>
    public const string DiagnosticId = "NoSpaceAfterBrace";

    private const string Category = Categories.Spacing;
    private static readonly DiagnosticDescriptor Rule = NewRule();

    /// <inheritdoc/>
    public override ImmutableArray<DiagnosticDescriptor>
        SupportedDiagnostics => [Rule];

    /// <inheritdoc/>
    private protected override void Register(AnalysisContext context)
    {
        context.EnableConcurrentExecution();
        context.RegisterSyntaxTreeAction(SyntaxTreeAction);
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

    private static void SyntaxTreeAction(SyntaxTreeAnalysisContext context)
    {
        static bool StartsWithSpace(SyntaxTriviaList list)
        {
            return list.First()
                .IsKindOneOf(
                    SyntaxKind.WhitespaceTrivia,
                    SyntaxKind.EndOfLineTrivia);
        }

        static bool DoesBraceNeedSpace(
            SyntaxToken token, Func<SyntaxToken, bool> otherwiseNext)
        {
            return token.HasTrailingTrivia
                ? !StartsWithSpace(token.TrailingTrivia)
                : token.GetNextToken() is {} next
                    && next.HasLeadingTrivia
                ? !StartsWithSpace(next.LeadingTrivia)
                : otherwiseNext(next);
        }

        static bool DoesCloseBraceNeedSpace(SyntaxToken token)
        {
            return DoesBraceNeedSpace(
                token,
                next => !next.IsKindOneOf(
                        SyntaxKind.CloseParenToken,
                        SyntaxKind.CommaToken,
                        SyntaxKind.DotToken,
                        SyntaxKind.ExclamationToken,
                        SyntaxKind.None,
                        SyntaxKind.OpenBracketToken,
                        SyntaxKind.QuestionToken)
                    && !(next.IsKind(SyntaxKind.SemicolonToken)
                        && next.Parent is not EmptyStatementSyntax));
        }

        static bool DoesOpenBraceNeedSpace(SyntaxToken token)
        {
            return DoesBraceNeedSpace(
                token,
                next => !next.IsKindOneOf(
                    SyntaxKind.None,
                    SyntaxKind.CloseBraceToken));
        }

        static bool IsExceptional(SyntaxToken token)
        {
            return token.IsMissing
                || token.Parent.IsKind(SyntaxKind.Interpolation);
        }

        var root = context.Tree
            .GetCompilationUnitRoot(context.CancellationToken);
        var leftBraces = root.DescendantTokens()
            .Where(t => t.IsKind(SyntaxKind.OpenBraceToken)
                && !IsExceptional(t)
                && DoesOpenBraceNeedSpace(t));
        var rightBraces = root.DescendantTokens()
            .Where(t => t.IsKind(SyntaxKind.CloseBraceToken)
                && !IsExceptional(t)
                && DoesCloseBraceNeedSpace(t));
        var all = leftBraces.Concat(rightBraces)
            .Select(t => Diagnostic.Create(Rule, t.GetLocation(), t.Text))
            .ToList();
        foreach (var d in all)
        {
            context.ReportDiagnostic(d);
        }
    }
}
