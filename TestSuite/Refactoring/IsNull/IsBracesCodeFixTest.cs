namespace TestSuite.Refactoring.IsNull;

using BeliefCrucible;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StyleChecker.Analyzers.Refactoring.IsNull;
using StyleChecker.CodeFixes.Refactoring.IsNull;

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
                $"Use '{token}' operator instead of 'is' pattern "
                + "matching.",
                DiagnosticSeverity.Info);
        }

        VerifyDiagnosticAndFix(code, Atmosphere.Default, Expected, fix);
    }
}
