namespace BeliefCrucible;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Maroontress.Roastery;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Superclass of all Unit tests made for diagnostics with CodeFixes. Contains
/// methods used to verify correctness of CodeFixes.
/// </summary>
/// <param name="analyer">
/// The diagnostic analyzer being tested.
/// </param>
/// <param name="codeFixProvider">
/// The CodeFix provider being tested.
/// </param>
public abstract class CodeFixVerifier(
    DiagnosticAnalyzer analyer,
    CodeFixProvider codeFixProvider) : DiagnosticVerifier(analyer)
{
    private static readonly string NewLine = Environment.NewLine;

    /// <summary>
    /// Gets the CodeFix provider being tested.
    /// </summary>
    /// <returns>
    /// The CodeFix provider being tested.
    /// </returns>
    protected CodeFixProvider CodeFixProvider { get; } = codeFixProvider;

    /// <summary>
    /// Creates a new <c>CodeChange</c> from entire texts representing for the
    /// specified file and the fixed file.
    /// </summary>
    /// <param name="name">
    /// The name of the source file to be read on the base directory. The
    /// extension ".cs" is not needed. For example, if the name is
    /// <c>"Foo"</c>, the two files <c>Foo.cs</c> and <c>Foo_Fixed.cs</c>
    /// are to be read.
    /// </param>
    /// <returns>
    /// The <c>CodeChange</c> object.
    /// </returns>
    protected CodeChange NewCodeChange(string name)
        => CodeChange.Of(name, n => ReadText(n));

    /// <summary>
    /// Tests the analyzer and CodeFix provider. Verifies each of diagnostics
    /// found in the specified source, compared with the result the specified
    /// function extracts from the beliefs embedded from the source. And then,
    /// applies the CodeFix to the decoded source, comparing the result with
    /// the specified expected source.
    /// </summary>
    /// <param name="encodedSource">
    /// The encoded source where the beliefs have been embedded.
    /// </param>
    /// <param name="atmosphere">
    /// The compilation environment.
    /// </param>
    /// <param name="toResult">
    /// The function that returns the expected diagnostic result with the
    /// specified belief.
    /// </param>
    /// <param name="codeFix">
    /// A expected source to be equal to the decoded source to which the
    /// CodeFix is applied.
    /// </param>
    protected void VerifyDiagnosticAndFix(
        string encodedSource,
        Atmosphere atmosphere,
        Func<Belief, Result> toResult,
        string codeFix)
    {
        var (source, expected) = Beliefs.Decode(
            encodedSource, atmosphere.ExcludeIds, toResult);
        try
        {
            VerifyDiagnostics(atmosphere, expected, source);
            VerifyFix(new CodeChange(source, codeFix));
        }
        catch (CompilationException)
        {
            Trace.WriteLine(source);
            throw;
        }
    }

    /// <summary>
    /// Tests the analyzer and CodeFix provider. Verifies each of diagnostics
    /// found in the specified source, compared with the result the specified
    /// function extracts from the beliefs embedded from the source. And then,
    /// applies the CodeFix to the decoded source, comparing the result with
    /// the specified expected source.
    /// </summary>
    /// <param name="change">
    /// The code change object. The <see cref="CodeChange.Before"/> is an
    /// encoded source where the beliefs have been embedded. The <see
    /// cref="CodeChange.After"/> is a expected source to be equal to the
    /// decoded source to which the CodeFix is applied.
    /// </param>
    /// <param name="atmosphere">
    /// The compilation environment.
    /// </param>
    /// <param name="toResult">
    /// The function that returns the expected diagnostic result with the
    /// specified belief.
    /// </param>
    protected void VerifyDiagnosticAndFix(
        CodeChange change,
        Atmosphere atmosphere,
        Func<Belief, Result> toResult)
    {
        VerifyDiagnosticAndFix(
            change.Before, atmosphere, toResult, change.After);
    }

    /// <summary>
    /// Called to test a C# CodeFix when applied on the inputted string as a
    /// source.
    /// </summary>
    /// <param name="change">
    /// A code change object. The <see cref="CodeChange.Before"/> is a class in
    /// the form of a string before the CodeFix was applied to it. The <see
    /// cref="CodeChange.After"/> is a class in the form of a string after the
    /// CodeFix was applied to it.
    /// </param>
    /// <param name="allowNewCompilerDiagnostics">
    /// A <c>bool</c> controlling whether or not the test will fail if the
    /// CodeFix introduces other warnings after being applied.
    /// </param>
    protected void VerifyFix(
        CodeChange change,
        bool allowNewCompilerDiagnostics = false)
    {
        VerifyFix([change], allowNewCompilerDiagnostics);
    }

    /// <summary>
    /// Verifies the result of CodeFixes. Creates a <c>Document</c>s from the
    /// <c>CodeChange</c>s, then gets diagnostics on it and applies the
    /// relevant CodeFixes. Then gets the string after the CodeFix is applied
    /// and compares it with the expected result. Note: If any CodeFix causes
    /// new diagnostics to show up, the test fails unless <paramref
    /// name="allowNewCompilerDiagnostics"/> is set to true.
    /// </summary>
    /// <param name="codeChanges">
    /// The sources in the form of a string before/after the CodeFix was
    /// applied to it.
    /// </param>
    /// <param name="allowNewCompilerDiagnostics">
    /// A <c>bool</c> controlling whether or not the test will fail if the
    /// CodeFix introduces other warnings after being applied.
    /// </param>
    protected void VerifyFix(
        IEnumerable<CodeChange> codeChanges,
        bool allowNewCompilerDiagnostics = false)
    {
        var analyzer = DiagnosticAnalyzer;
        var codeFixProvider = CodeFixProvider;
        var applier = new FixApplier(analyzer, codeFixProvider);

        var expectedMap = new Dictionary<DocumentId, string>();
        var project = Projects.Of(
            codeChanges,
            (id, c) => expectedMap[id] = c.After);
        var documents = project.Documents.ToArray();
        var firstApplierContext = applier.Analyze(documents);
        var maxTryCount = firstApplierContext.AnalyzerDiagnostics.Length;
        Assert.IsTrue(maxTryCount > 0);

        var verifyContext = firstApplierContext;
        var newDocumentMap = applier.Modify(verifyContext);
        for (var k = 0; k < maxTryCount; ++k)
        {
            var newDocuments = newDocumentMap.Values.ToArray();
            var newApplierContext = applier.Analyze(newDocuments);
            var newCompilerDiagnostics
                = newApplierContext.CompilerDiagnostics;
            var diagnosticsDelta = Diagnostics.GetNewDelta(
                firstApplierContext.CompilerDiagnostics,
                newCompilerDiagnostics);

            // Checks if applying the code fix introduced any new compiler
            // diagnostics.
            if (!allowNewCompilerDiagnostics && diagnosticsDelta.Any())
            {
                FailFixIntroducedNewCompilerDiagnostics(
                    newDocuments, newCompilerDiagnostics);
            }

            verifyContext = newApplierContext;
            if (!verifyContext.AnalyzerDiagnostics.Any())
            {
                break;
            }
            newDocumentMap = applier.Modify(verifyContext);
        }
        Assert.IsFalse(verifyContext.AnalyzerDiagnostics.Any());

        foreach (var id in project.DocumentIds)
        {
            var actual = ToString(newDocumentMap[id]);
            var expected = expectedMap[id];
            Compare(id, actual, expected);
        }
    }

    private static void FailFixIntroducedNewCompilerDiagnostics(
        IEnumerable<Document> newDocuments,
        IEnumerable<Diagnostic> compilerDiagnostics)
    {
        static Document ToFormat(Document d)
        {
            if (d.GetSyntaxRootAsync().Result is not {} root)
            {
                return d;
            }
            var newRoot = Formatter.Format(
                root, Formatter.Annotation, d.Project.Solution.Workspace);
            return d.WithSyntaxRoot(newRoot);
        }

        // Format and get the compiler diagnostics again so that
        // the locations make sense in the output.
        var formattedDocuments = newDocuments.Select(ToFormat);
        var newCompilerDiagnostics = formattedDocuments
            .SelectMany(d => Documents.GetCompilerDiagnostics(d));
        var diagnosticsDelta = Diagnostics.GetNewDelta(
            compilerDiagnostics, newCompilerDiagnostics);

        var diagnosticMessages = string.Join(
            NewLine,
            diagnosticsDelta.Select(d => d.ToString()));
        if (formattedDocuments.FirstOrDefault(
            d => !d.SupportsSyntaxTree) is {} d)
        {
            throw new CompilationException(
                $"{d.FilePath}: the syntax tree is not supported");
        }
        var sources = string.Join(
            $"{NewLine}{NewLine}",
            formattedDocuments.Select(d => d.GetSyntaxRootAsync().Result)
                .FilterNonNullReference()
                .Select(d => d.ToFullString()));
        Assert.Fail(
            $"Fix introduced new compiler diagnostics:{NewLine}"
            + $"{diagnosticMessages}{NewLine}"
            + $"{NewLine}"
            + $"New documents:{NewLine}"
            + $"{sources}{NewLine}");
    }

    /// <summary>
    /// Compares the specified actual source and the specified expected source
    /// and then if there is any difference in them, asserts to fail the test
    /// case with showing the specified <c>DocumentId</c> and the location of
    /// the first difference.
    /// </summary>
    /// <param name="id">
    /// The <c>DocumentId</c> of the source to compare.
    /// </param>
    /// <param name="actual">
    /// The actual source that the CodeFix provider provides.
    /// </param>
    /// <param name="expected">
    /// The expected source that the CodeFix provider is supposed to provide.
    /// </param>
    private static void Compare(
        DocumentId id, string actual, string expected)
    {
        static string Hex(string s)
        {
            var all = s.Select(c => $"0x{Convert.ToInt32(c):X}");
            return string.Join(',', all);
        }

        static string WithHex(string s)
            => $"'{s}' ({Hex(s)})";

        static string Difference(string id, int line, string e)
            => $"id {id}: line {line + 1}:{NewLine}"
                + $"expected={WithHex(e)},{NewLine}"
                + $"  actual=";

        var actualArray = actual.Split(NewLine);
        var expectedArray = expected.Split(NewLine);
        var lines = actualArray.Length;
        var maybeFirst = Enumerable.Range(0, lines)
            .Where(k => expectedArray[k] != actualArray[k])
            .FirstValue();
        if (maybeFirst is {} first)
        {
            var e = expectedArray[first];
            var a = actualArray[first];

            // If Microsoft.CodeAnalysis.CSharp.Workspaces version 2.x is used,
            // Fotmatter.Format() places CRLF at End of Line on any platform.
            // (The version 3.0 is fixed.)
            Assert.Fail(Difference(id.ToString(), lines, e) + WithHex(a));
        }
        if (lines < expectedArray.Length)
        {
            var e = expectedArray[lines];
            Assert.Fail(Difference(id.ToString(), lines, e) + "EOF");
        }
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// Returns the string representing the specified document based on the
    /// syntax root.
    /// </summary>
    /// <param name="document">
    /// The <c>>Document</c> to be converted to a string.
    /// </param>
    /// <returns>
    /// A string containing the syntax of the Document after formatting.
    /// </returns>
    private static string ToString(Document document)
    {
        var newDocument = Simplifier.ReduceAsync(
                document, Simplifier.Annotation)
            .Result;
        if (newDocument.GetSyntaxRootAsync()
            .Result is not {} root)
        {
            throw new NullReferenceException(
                "The syntax root of the document is null");
        }
        var newRoot = Formatter.Format(
            root,
            Formatter.Annotation,
            newDocument.Project
                .Solution
                .Workspace);
        return newRoot.GetText()
            .ToString();
    }
}
