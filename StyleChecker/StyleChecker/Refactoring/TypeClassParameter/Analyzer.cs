namespace StyleChecker.Refactoring.TypeClassParameter
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Operations;
    using R = Resources;

    /// <summary>
    /// TypeClassParameter analyzer.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class Analyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID of this analyzer.
        /// </summary>
        public const string DiagnosticId = "TypeClassParameter";

        private const string Category = Categories.Refactoring;
        private static readonly DiagnosticDescriptor Rule = NewRule();

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor>
            SupportedDiagnostics => ImmutableArray.Create(Rule);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(
                GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(StartAction);
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

        private static bool IsOfType<T>(ITypeSymbol s)
            => s.ToString().Equals(typeof(T).FullName);

        private static IEnumerable<IParameterSymbol>
            TypeClassParameters(IMethodSymbol m)
                => m.Parameters.Where(p => IsOfType<Type>(p.Type)
                    && p.RefKind == RefKind.None
                    && !p.IsOptional);

        private static bool IsEveryArgumentTypeofOperator(
            IEnumerable<IInvocationOperation> invocations, int i)
            => invocations.Count() == invocations
                .Where(o => o.Arguments.Length > i)
                .Select(o => o.Arguments[i].Value)
                .OfType<ITypeOfOperation>()
                .Count();

        private void StartAction(
            CompilationStartAnalysisContext context)
        {
            var globalMethods = new List<IMethodSymbol>();
            var globalInvocations = new List<IInvocationOperation>();
            context.RegisterSemanticModelAction(
                c => AnalyzeModel(c, globalMethods, globalInvocations));
            context.RegisterCompilationEndAction(
                c => AnalyzeGlobal(c, globalMethods, globalInvocations));
        }

        private void AnalyzeGlobal(
            CompilationAnalysisContext context,
            List<IMethodSymbol> globalMethods,
            List<IInvocationOperation> globalInvocations)
        {
            ImmutableArray<T> ToImmutableArray<T>(IEnumerable<T> a)
            {
                lock (a)
                {
                    return a.ToImmutableArray();
                }
            }

            var allMethods = ToImmutableArray(globalMethods);
            var allInvocations = ToImmutableArray(globalInvocations);

            foreach (var m in allMethods)
            {
                var invocations = allInvocations
                    .Where(o => o.TargetMethod.OriginalDefinition.Equals(m));
                if (!invocations.Any())
                {
                    continue;
                }

                bool IsTypeofArgumentForAll(int i)
                    => IsEveryArgumentTypeofOperator(invocations, i);

                var all = TypeClassParameters(m)
                    .Select(p => p.Ordinal)
                    .Where(IsTypeofArgumentForAll);
                foreach (var index in all)
                {
                    var parameter = m.Parameters[index];
                    var location = parameter.Locations.FirstOrDefault();
                    if (location == null)
                    {
                        continue;
                    }
                    var diagnostic = Diagnostic.Create(
                        Rule,
                        location,
                        parameter.Name,
                        R.Method,
                        m.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private void AnalyzeModel(
            SemanticModelAnalysisContext context,
            List<IMethodSymbol> globalMethods,
            List<IInvocationOperation> globalInvocations)
        {
            bool HasTypeClassParameter(IMethodSymbol m)
                => TypeClassParameters(m).Any();

            var operationSupplier = context.GetOperationSupplier();
            var symbolizer = context.GetSymbolizer();

            var root = context.GetCompilationUnitRoot();
            var allNodes = root.DescendantNodes();
            var localFunctions = allNodes
                .OfType<LocalFunctionStatementSyntax>()
                .Select(operationSupplier)
                .OfType<ILocalFunctionOperation>()
                .Select(o => o.Symbol)
                .Where(m => HasTypeClassParameter(m));

            var unitMethods = allNodes
                .OfType<MethodDeclarationSyntax>()
                .Select(symbolizer.ToSymbol)
                .OfType<IMethodSymbol>()
                .Where(m => !m.IsAbstract
                    && !m.IsExtern
                    && m.PartialDefinitionPart == null
                    && m.PartialImplementationPart == null
                    && m.ContainingType.TypeKind != TypeKind.Interface)
                .Where(HasTypeClassParameter);
            lock (globalMethods)
            {
                globalMethods.AddRange(unitMethods);
            }

            var unitInvovations = allNodes
                .OfType<InvocationExpressionSyntax>()
                .Select(operationSupplier)
                .OfType<IInvocationOperation>()
                .Where(o => o.TargetMethod.MethodKind == MethodKind.Ordinary);
            lock (globalInvocations)
            {
                globalInvocations.AddRange(unitInvovations);
            }

            foreach (var m in localFunctions)
            {
                if (!(m.ContainingSymbol is IMethodSymbol containingMethod))
                {
                    continue;
                }
                var node = containingMethod.DeclaringSyntaxReferences
                    .FirstOrDefault();
                if (node == null)
                {
                    continue;
                }
                var invocations = node.GetSyntax().DescendantNodes()
                    .OfType<InvocationExpressionSyntax>()
                    .Select(operationSupplier)
                    .OfType<IInvocationOperation>()
                    .Where(o => o.TargetMethod.OriginalDefinition.Equals(m));
                if (!invocations.Any())
                {
                    continue;
                }

                bool IsTypeofArgumentForAll(int i)
                    => IsEveryArgumentTypeofOperator(invocations, i);

                var all = TypeClassParameters(m)
                    .Select(p => p.Ordinal)
                    .Where(IsTypeofArgumentForAll);
                foreach (var index in all)
                {
                    var parameter = m.Parameters[index];
                    var location = parameter.Locations.FirstOrDefault();
                    if (location == null)
                    {
                        continue;
                    }
                    var diagnostic = Diagnostic.Create(
                        Rule,
                        location,
                        parameter.Name,
                        R.LocalFunction,
                        m.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}