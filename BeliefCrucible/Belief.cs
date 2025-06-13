namespace BeliefCrucible;

using System;
using System.Collections.Generic;
using Maroontress.Roastery;
using Microsoft.CodeAnalysis;

/// <summary>
/// The belief is a metadata embedded with the source code as a comment, which
/// represents the expected diagnostics. It can be used to create the
/// diagnostic result.
/// </summary>
/// <param name="Row">
/// The row.
/// </param>
/// <param name="Column">
/// The column.
/// </param>
/// <param name="Message">
/// The message.
/// </param>
/// <param name="DeltaRow">
/// The delta row.
/// </param>
public readonly record struct Belief(
    int Row, int Column, string Message, int DeltaRow)
{
    /// <summary>
    /// Returns a new <see cref="Belief"/> with the specified row.
    /// </summary>
    /// <param name="newRow">
    /// The row.
    /// </param>
    /// <returns>
    /// A new belief.
    /// </returns>
    public Belief WithRow(int newRow)
        => this with { Row = newRow };

    /// <summary>
    /// Returns a new string representing the substituted message with the
    /// specified mapping function.
    /// </summary>
    /// <param name="map">
    /// A function to map a key to the value.
    /// </param>
    /// <returns>
    /// A new substituted message.
    /// </returns>
    public string Substitute(Func<string, string> map)
        => TextTemplates.Substitute(Message, map);

    /// <summary>
    /// Returns a new result with the specified analyzer's ID and the message
    /// created with the specified function.
    /// </summary>
    /// <param name="id">
    /// The analyzer's ID.
    /// </param>
    /// <param name="toMessage">
    /// A function returns the result message with the belief message.
    /// </param>
    /// <param name="serverity">
    /// The severity.
    /// </param>
    /// <returns>
    /// The new result.
    /// </returns>
    public Result ToResult(
            string id,
            Func<string, string> toMessage,
            DiagnosticSeverity serverity = DiagnosticSeverity.Warning)
        => new(SingleLocation(), id, toMessage(Message), serverity);

    /// <summary>
    /// Returns a new result with the specified analyzer's ID and the specified
    /// message.
    /// </summary>
    /// <param name="id">
    /// The analyzer's ID.
    /// </param>
    /// <param name="message">
    /// The result message.
    /// </param>
    /// <param name="serverity">
    /// The severity.
    /// </param>
    /// <returns>
    /// The new result.
    /// </returns>
    public Result ToResult(
            string id,
            string message,
            DiagnosticSeverity serverity = DiagnosticSeverity.Warning)
        => new(SingleLocation(), id, message, serverity);

    /// <summary>
    /// Returns a new array of <see cref="ResultLocation"/> containing the
    /// single element representing the location of this belief.
    /// </summary>
    /// <returns>
    /// A new array of <see cref="ResultLocation"/> containing the single
    /// element.
    /// </returns>
    private IEnumerable<ResultLocation> SingleLocation()
        => [new ResultLocation("Test0.cs", Row, Column)];
}
