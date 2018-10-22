namespace StyleChecker.Refactoring.IneffectiveReadByte
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Formatting;
    using R = Resources;

    /// <summary>
    /// PostIncrement code fix provider.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CodeFixer))]
    [Shared]
    public sealed class CodeFixer : CodeFixProvider
    {
        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds
            => ImmutableArray.Create(Analyzer.DiagnosticId);

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider()
            => WellKnownFixAllProviders.BatchFixer;

        /// <inheritdoc/>
        public override async Task RegisterCodeFixesAsync(
            CodeFixContext context)
        {
            var localize = Localizers.Of(R.ResourceManager, typeof(R));
            var title = localize(nameof(R.FixTitle)).ToString();

            var root = await context
                .Document.GetSyntaxRootAsync(context.CancellationToken)
                .ConfigureAwait(false);

            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var properties = diagnostic.Properties;

            var node = root.FindNode(diagnosticSpan);

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: title,
                    createChangedDocument:
                        c => Replace(context.Document, node, properties, c),
                    equivalenceKey: title),
                diagnostic);
        }

        private async Task<Document> Replace(
            Document document,
            SyntaxNode node,
            ImmutableDictionary<string, string> properties,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken)
                .ConfigureAwait(false);

            var binaryReader = properties["instance"];
            var byteArray = properties["array"];
            var offset = properties["offset"];
            var length = properties["length"];
            var s1 = SyntaxFactory.ParseStatement(""
                + "System.Action<byte[], int, int> _readFully = (_array, _offset, _length) => {\r\n"
                + $"    var _reader = {binaryReader};\r\n"
                + "    while (_length > 0)\r\n"
                + "    {\r\n"
                + "        var _size = _reader.Read(_array, _offset, _length);\r\n"
                + "        if (_size == 0)\r\n"
                + "        {\r\n"
                + "            throw new System.IO.EndOfStreamException();\r\n"
                + "        }\r\n"
                + "        _offset += _size;\r\n"
                + "        _length -= _size;\r\n"
                + "    }\r\n"
                + "};\r\n");
            var s2 = SyntaxFactory.ParseStatement(""
                + $"_readFully({byteArray}, {offset}, {length});\r\n");

            var solution = document.Project.Solution;
            var workspace = solution.Workspace;

            var statements = new[] { s1, s2 }
                .Select(s => s.WithAdditionalAnnotations(Formatter.Annotation));
            var newNode = SyntaxFactory.Block(statements)
                .WithLeadingTrivia(node.GetLeadingTrivia())
                .WithTrailingTrivia(node.GetTrailingTrivia())
                .WithAdditionalAnnotations(Formatter.Annotation);

            var formattedNode = Formatter.Format(
               newNode,
               Formatter.Annotation,
               workspace,
               workspace.Options);
            var newRoot = root.ReplaceNode(node, formattedNode);
            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }
    }
}
