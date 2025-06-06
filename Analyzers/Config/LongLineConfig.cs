namespace StyleChecker.Analyzers.Config;

using System.Collections.Generic;
using Maroontress.Oxbind;

/// <summary>
/// The configuration data of LongLine analyzer.
/// </summary>
[ForElement("LongLine", Namespace)]
public sealed class LongLineConfig(
    [ForAttribute("maxLineLength")] BindResult<string>? maxLineLengthResult)
    : AbstractConfig
{
    private const int DefaultMaxLineLength = 80;

    /// <summary>
    /// Initializes a new instance of the <see cref="LongLineConfig"/> class.
    /// </summary>
    [Ignored]
    public LongLineConfig()
        : this(null)
    {
    }

    private BindResult<string>? MaxLineLengthEvent { get; }
        = maxLineLengthResult;

    /// <inheritdoc/>
    public override IEnumerable<WhereWhy> Validate()
        => ParseKit.ValidateInt(
            MaxLineLengthEvent,
            v => v > 0,
            "invalid integer value of maxLineLength attribute",
            "non-positive integer value of maxLineLength attribute");

    /// <summary>
    /// Gets the maximum line length.
    /// </summary>
    /// <seealso cref="Size.LongLine.Analyzer"/>
    /// <returns>
    /// The maximum line length.
    /// </returns>
    public int GetMaxLineLength() => ParseKit.ToIntValue(
        MaxLineLengthEvent,
        DefaultMaxLineLength,
        v => v > 0);
}
