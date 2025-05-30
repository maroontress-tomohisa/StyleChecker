namespace TestSuite.Refactoring.UnnecessaryUsing;

using BeliefCrucible;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StyleChecker.Analyzers.Refactoring.UnnecessaryUsing;
using StyleChecker.CodeFixes.Refactoring.UnnecessaryUsing;

[TestClass]
public sealed class AnalyzerTest : CodeFixVerifier
{
    public AnalyzerTest()
        : base(new Analyzer(), new CodeFixer())
    {
    }

    [TestMethod]
    public void Code()
    {
        var code = ReadText("Code");
        var fix = ReadText("CodeFix");
        static Result Expected(Belief b) => b.ToResult(
            Analyzer.DiagnosticId,
            m => $"The using statement is not necessary for '{m}'.");

        VerifyDiagnosticAndFix(code, Atmosphere.Default, Expected, fix);
    }
}
