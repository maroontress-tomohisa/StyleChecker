namespace TestSuite.Settings.InvalidConfig;

using BeliefCrucible;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StyleChecker.Analyzers.Settings.InvalidConfig;

[TestClass]
public sealed class AnalyzerTest : DiagnosticVerifier
{
    public AnalyzerTest()
        : base(new Analyzer())
    {
    }

    [TestMethod]
    public void Empty()
        => VerifyDiagnostic("", Atmosphere.Default);

    [TestMethod]
    public void Okay()
    {
        var code = ReadText("Code");
        var configText = ReadText("Okay", "xml");
        var atmosphere = Atmosphere.Default
            .WithForceLocationValid(true)
            .WithConfigText(configText);

        VerifyDiagnostic(code, atmosphere, []);
    }

    [TestMethod]
    public void ConfigElementNotClosed()
    {
        var code = ReadText("Code");
        var configText = ReadText("ConfigElementNotClosed", "xml");
        var atmosphere = Atmosphere.Default
            .WithForceLocationValid(true)
            .WithConfigText(configText);
        var result = NewErrorResult(
            NewLocations(5, 1),
            "InvalidConfig",
            """
            Unexpected end of file has occurred. The following elements are not closed: config. Line 5, position 1.
            """);
        VerifyDiagnostic(code, atmosphere, result);
    }

    [TestMethod]
    public void ValidationErrorOfLongLine()
    {
        var code = ReadText("Code");
        var configText = ReadText("LongLineInvalidMaxLineLength", "xml");
        var atmosphere = Atmosphere.Default
            .WithForceLocationValid(true)
            .WithConfigText(configText);
        var result = NewErrorResult(
            NewLocations(5, 13),
            "InvalidConfig",
            """
            invalid integer value of maxLineLength attribute: 'a'
            """);
        VerifyDiagnostic(code, atmosphere, result);
    }

    [TestMethod]
    public void UnexpectedChildElement()
    {
        var code = ReadText("Code");
        var configText = ReadText("UnexpectedChildElement", "xml");
        var atmosphere = Atmosphere.Default
            .WithForceLocationValid(true)
            .WithConfigText(configText);
        var ns = "https://maroontress.com/StyleChecker/config.v1";
        var result = NewErrorResult(
            NewLocations(5, 4),
            "InvalidConfig",
            $"""
            Unexpected node type: Element of the element '{ns}:Unexpected'. (Expected the end of element '{ns}:config'.)
            """);
        VerifyDiagnostic(code, atmosphere, result);
    }

    [TestMethod]
    public void UnexpectedRootElement()
    {
        var code = ReadText("Code");
        var configText = ReadText("UnexpectedRootElement", "xml");
        var atmosphere = Atmosphere.Default
            .WithForceLocationValid(true)
            .WithConfigText(configText);
        var actualNs = "https://example.com/";
        var ns = "https://maroontress.com/StyleChecker/config.v1";
        var result = NewErrorResult(
            NewLocations(2, 2),
            "InvalidConfig",
            $"""
            Unexpected node type: Element of the element '{actualNs}:Unexpected'. (Expected the start of element '{ns}:config'.)
            """);
        VerifyDiagnostic(code, atmosphere, result);
    }

    private static ResultLocation[] NewLocations(int row, int col)
    {
        return [new ResultLocation("StyleChecker.xml", row, col)];
    }

    private static Result NewErrorResult(
            ResultLocation[] locations, string id, string message)
        => new(locations, id, message, DiagnosticSeverity.Error);
}
