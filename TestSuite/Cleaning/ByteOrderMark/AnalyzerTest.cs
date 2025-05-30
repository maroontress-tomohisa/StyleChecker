namespace TestSuite.Cleaning.ByteOrderMark;

using System.Collections.Generic;
using System.IO;
using System.Text;
using BeliefCrucible;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StyleChecker.Analyzers.Cleaning.ByteOrderMark;

[TestClass]
public sealed class AnalyzerTest : DiagnosticVerifier
{
    public AnalyzerTest()
        : base(new Analyzer())
    {
    }

    [TestMethod]
    public void NotFound()
    {
        var code = "";
        var path = Path.Combine(BaseDir, "Test0.cs");
        File.Delete(path);

        var atmosphere = Atmosphere.Default
            .WithBasePath(BaseDir)
            .WithForceLocationValid(true);
        VerifyDiagnostic(code, atmosphere);
    }

    [TestMethod]
    public void Empty()
    {
        var code = "";
        var path = Path.Combine(BaseDir, "Test0.cs");
        File.WriteAllText(path, code, Encoding.ASCII);

        var atmosphere = Atmosphere.Default
            .WithBasePath(BaseDir);
        VerifyDiagnostic(code, atmosphere);
    }

    [TestMethod]
    public void Okay()
    {
        var binPath = Path.Combine(BaseDir, "Okay.bin");
        var code = File.ReadAllText(binPath);
        var bin = File.ReadAllBytes(binPath);
        var path = Path.Combine(BaseDir, "Test0.cs");
        File.WriteAllBytes(path, bin);

        var atmosphere = Atmosphere.Default
            .WithBasePath(BaseDir);
        VerifyDiagnostic(code, atmosphere);
    }

    [TestMethod]
    public void OkayCustomFile()
    {
        var binPath = Path.Combine(BaseDir, "Okay.bin");
        var code = File.ReadAllText(binPath);
        var bin = File.ReadAllBytes(binPath);
        var path = Path.Combine(BaseDir, "Test0.cs");
        File.WriteAllBytes(path, bin);

        var customPath = Path.Combine(BaseDir, "CustomFile");
        File.WriteAllBytes(
            customPath,
            File.ReadAllBytes(Path.Combine(BaseDir, "Okay.bin")));

        var configText = ReadText("SpecifyCustomFile", "xml");

        var atmosphere = Atmosphere.Default
            .WithConfigText(configText)
            .WithBasePath(BaseDir);
        VerifyDiagnostic(code, atmosphere);
    }

    [TestMethod]
    public void FileStartsWithBom()
    {
        var binPath = Path.Combine(BaseDir, "Code.bin");
        var code = File.ReadAllText(binPath);
        var bin = File.ReadAllBytes(binPath);
        var path = Path.Combine(BaseDir, "Test0.cs");
        File.WriteAllBytes(path, bin);

        var atmosphere = Atmosphere.Default
            .WithBasePath(BaseDir)
            .WithForceLocationValid(true);
        var result = NewErrorResult(
            NewLocations(1, 1),
            Analyzer.DiagnosticId,
            $"The BOM in UTF-8 encoding must be removed: {path}");
        VerifyDiagnostic(code, atmosphere, result);
    }

    [TestMethod]
    public void CustomFileStartsWithBom()
    {
        var binPath = Path.Combine(BaseDir, "Okay.bin");
        var code = File.ReadAllText(binPath);
        var bin = File.ReadAllBytes(binPath);
        var path = Path.Combine(BaseDir, "Test0.cs");
        File.WriteAllBytes(path, bin);

        var customPath = Path.Combine(BaseDir, "CustomFile");
        File.WriteAllBytes(
            customPath,
            File.ReadAllBytes(Path.Combine(BaseDir, "Code.bin")));

        var configText = ReadText("SpecifyCustomFile", "xml");

        var atmosphere = Atmosphere.Default
            .WithConfigText(configText)
            .WithBasePath(BaseDir);

        var result = NewErrorResult(
            NewNoLocations(),
            Analyzer.DiagnosticId,
            $"The BOM in UTF-8 encoding must be removed: {customPath}");
        VerifyDiagnostic(code, atmosphere, result);
    }

    private static IEnumerable<ResultLocation> NewNoLocations()
        => [new ResultLocation(null, -1, -1)];

    private static Result NewErrorResult(
            IEnumerable<ResultLocation> locations,
            string id,
            string message,
            DiagnosticSeverity severity = DiagnosticSeverity.Warning)
        => new(locations, id, message, severity);

    private IEnumerable<ResultLocation> NewLocations(int row, int col)
    {
        var path = Path.Combine(BaseDir, "Test0.cs");
        return [new ResultLocation(path, row, col)];
    }
}
