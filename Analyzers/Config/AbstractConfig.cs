namespace StyleChecker.Analyzers.Config;

using System.Collections.Generic;

/// <summary>
/// Base configuration class.
/// </summary>
public abstract class AbstractConfig : Validateable
{
    /// <summary>
    /// The default namespace of the configuration XML document.
    /// </summary>
    public const string Namespace
        = "https://maroontress.com/StyleChecker/config.v1";

    /// <inheritdoc/>
    public abstract IEnumerable<WhereWhy> Validate();
}
