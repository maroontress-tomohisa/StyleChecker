namespace StyleChecker.Test.Refactoring.EmptyArrayCreation;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using StyleChecker.Refactoring.EmptyArrayCreation;
using StyleChecker.Test.Framework;

[TestClass]
public sealed class AnalyzerTest : CodeFixVerifier
{
    public AnalyzerTest()
        : base(new Analyzer(), new CodeFixer())
    {
    }

    [TestMethod]
    public void Empty()
        => VerifyDiagnostic("", Atmosphere.Default);

    [TestMethod]
    public void Okay()
        => VerifyDiagnostic(ReadText("Okay"), Atmosphere.Default);

    [TestMethod]
    public void Code()
    {
        var code = ReadText("Code");
        var fix = ReadText("CodeFix");
        static Result Expected(Belief b) => b.ToResult(
            Analyzer.DiagnosticId,
            $"Use 'System.Array.Empty<{b.Message}>()' instead of an empty "
            + "array creation.");

        VerifyDiagnosticAndFix(code, Atmosphere.Default, Expected, fix);
    }
}
