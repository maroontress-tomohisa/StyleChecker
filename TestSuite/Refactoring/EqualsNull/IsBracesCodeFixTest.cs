namespace TestSuite.Refactoring.EqualsNull;

using BeliefCrucible;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StyleChecker.Analyzers.Refactoring.EqualsNull;
using StyleChecker.CodeFixes.Refactoring.EqualsNull;

[TestClass]
public sealed class IsBracesCodeFixTest : CodeFixVerifier
{
    public IsBracesCodeFixTest()
        : base(new Analyzer(), new IsBracesCodeFixer())
    {
    }

    [TestMethod]
    public void Code()
    {
        var code = ReadText("Code");
        var fix = ReadText("IsBracesCodeFix");
        static Result Expected(Belief b)
        {
            var token = b.Message;
            return b.ToResult(
                Analyzer.DiagnosticId,
                $"Use 'is' pattern matching instead of '{token}' operator.",
                DiagnosticSeverity.Info);
        }

        VerifyDiagnosticAndFix(code, Atmosphere.Default, Expected, fix);
    }
}
