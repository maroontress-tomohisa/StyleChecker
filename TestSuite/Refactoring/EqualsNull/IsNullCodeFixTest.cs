namespace TestSuite.Refactoring.EqualsNull;

using BeliefCrucible;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StyleChecker.Analyzers.Refactoring.EqualsNull;
using StyleChecker.CodeFixes.Refactoring.EqualsNull;

[TestClass]
public sealed class IsNullCodeFixTest : CodeFixVerifier
{
    public IsNullCodeFixTest()
        : base(new Analyzer(), new IsNullCodeFixer())
    {
    }

    [TestMethod]
    public void Code()
    {
        var code = ReadText("Code");
        var fix = ReadText("IsNullCodeFix");
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
