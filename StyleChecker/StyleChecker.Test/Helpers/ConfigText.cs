namespace TestHelper
{
    using System.Collections.Immutable;
    using System.Threading;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;
    using Microsoft.CodeAnalysis.Text;

    /// <summary>
    /// Represents content of the configuration file.
    /// </summary>
    public sealed class ConfigText : AdditionalText
    {
        private readonly string text;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigText"/> class.
        /// </summary>
        /// <param name="text">
        /// The text representing content of the configuration file.
        /// </param>
        public ConfigText(string text)
        {
            this.text = text;
        }

        /// <inheritdoc/>
        public override string Path => "StyleChecker.xml";

        /// <summary>
        /// Returns a new <c>AnalyzerOptions</c> including the additional file
        /// whose content represents the specified text.
        /// </summary>
        /// <param name="text">
        /// The text representing content of the configuration file.
        /// </param>
        /// <returns>
        /// The new <c>AnalyzerOptions</c>.
        /// </returns>
        public static AnalyzerOptions ToAnalyzerOptions(string text)
        {
            var additionalFiles = ImmutableArray.Create(
                new ConfigText(text) as AdditionalText);
            return new AnalyzerOptions(additionalFiles);
        }

        /// <inheritdoc/>
        public override SourceText GetText(
            CancellationToken cancellationToken)
        {
            return SourceText.From(text);
        }
    }
}
