namespace StyleChecker.Size.LongLine
{
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;
    using StyleChecker.Settings;
    using R = Resources;

    /// <summary>
    /// LongLine analyzer.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class Analyzer : AbstractAnalyzer
    {
        /// <summary>
        /// The ID of this analyzer.
        /// </summary>
        public const string DiagnosticId = "LongLine";

        private const string Category = Categories.Size;
        private static readonly DiagnosticDescriptor Rule = NewRule();

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor>
            SupportedDiagnostics => ImmutableArray.Create(Rule);

        /// <inheritdoc/>
        private protected override void Register(AnalysisContext context)
        {
            ConfigBank.LoadRootConfig(context, StartAction);
            context.EnableConcurrentExecution();
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

        private static void AnalyzeSyntaxTree(
            SyntaxTreeAnalysisContext context,
            ConfigPod pod)
        {
            var config = pod.RootConfig.LongLine;
            var maxLineLength = config.GetMaxLineLength();

            bool Over(Location location)
                => location.GetLineSpan()
                    .StartLinePosition
                    .Character >= maxLineLength;

            var root = context.Tree.GetCompilationUnitRoot(
                context.CancellationToken);
            var firstTrivia = root.DescendantTrivia()
                .FirstOrDefault(t => t.IsKind(SyntaxKind.EndOfLineTrivia)
                    && Over(t.GetLocation()));
            var firstToken = root.DescendantTokens(descendIntoTrivia: true)
                .FirstOrDefault(
                    t => t.IsKind(SyntaxKind.XmlTextLiteralNewLineToken)
                    && Over(t.GetLocation()));
            var list = new List<Location>();
            if (firstTrivia != default)
            {
                list.Add(firstTrivia.GetLocation());
            }
            if (firstToken != default)
            {
                list.Add(firstToken.GetLocation());
            }
            if (list.Count == 0)
            {
                return;
            }
            list.Sort((location, another)
                => location.SourceSpan.Start - another.SourceSpan.Start);
            /* list[0] is safe. */
            var first = list[0];
            var diagnostic = Diagnostic.Create(
                Rule,
                first,
                maxLineLength);
            context.ReportDiagnostic(diagnostic);
        }

        private void StartAction(
            CompilationStartAnalysisContext context, ConfigPod pod)
        {
            context.RegisterSyntaxTreeAction(
                c => AnalyzeSyntaxTree(c, pod));
        }
    }
}
