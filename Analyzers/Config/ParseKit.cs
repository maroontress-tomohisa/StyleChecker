namespace StyleChecker.Analyzers.Config;

using System;
using System.Collections.Generic;
using Maroontress.Oxbind;

/// <summary>
/// Provides utilities for parsing values.
/// </summary>
public static class ParseKit
{
    /// <summary>
    /// Represents that there is no error.
    /// </summary>
    public static readonly IEnumerable<WhereWhy> NoError = [];

    private static readonly IReadOnlyDictionary<string, bool> BooleanMap
            = new Dictionary<string, bool>()
        {
            ["0"] = false,
            ["false"] = false,
            ["1"] = true,
            ["true"] = true,
        };

    /// <summary>
    /// Gets the boolean value of the specified BindEvent&lt;string&gt; object.
    /// </summary>
    /// <param name="result">
    /// The BindResult&lt;string&gt; object that provides a boolean value.
    /// </param>
    /// <param name="defaultValue">
    /// The default value.
    /// </param>
    /// <returns>
    /// The boolean value if the specified BindEvent has a value and the value
    /// is parsed successfully and valid, the default value otherwise.
    /// </returns>
    public static bool ToBooleanValue(
        BindResult<string>? result,
        bool defaultValue)
    {
        return (result is null)
            ? defaultValue
            : ParseBoolean(result.Value) ?? defaultValue;
    }

    /// <summary>
    /// Gets the integer value of the specified BindEvent&lt;string&gt;
    /// object.
    /// </summary>
    /// <param name="ev">
    /// The BindResult&lt;string&gt; object that provides an integer value.
    /// </param>
    /// <param name="defaultValue">
    /// The default value.
    /// </param>
    /// <param name="isValidValue">
    /// The function that returns whether a value of the argument is valid
    /// or not.
    /// </param>
    /// <returns>
    /// The integer value if the specified BindEvent has a value and the
    /// value is parsed successfully and valid, the default value
    /// otherwise.
    /// </returns>
    public static int ToIntValue(
        BindResult<string>? ev,
        int defaultValue,
        Func<int, bool> isValidValue)
    {
        if (ev is null)
        {
            return defaultValue;
        }
        var v = ParseInt(ev.Value);
        return (v.HasValue && isValidValue(v.Value))
            ? v.Value
            : defaultValue;
    }

    /// <summary>
    /// Validates the specified BindEvent&lt;string&gt; object and gets the
    /// tuples representing the error information.
    /// </summary>
    /// <param name="result">
    /// The BindResult&lt;string&gt; object.
    /// </param>
    /// <param name="invalidBooleanValueError">
    /// The error message when it is unable to parse a boolean value.
    /// </param>
    /// <returns>
    /// <see cref="NoError"/> if the specified BindEvent&lt;string&gt;
    /// object can be parsed successfully. Otherwise, the errors.
    /// </returns>
    public static IEnumerable<WhereWhy> ValidateBoolean(
        BindResult<string>? result,
        string invalidBooleanValueError)
    {
        if (result is null)
        {
            return NoError;
        }
        var v = ParseBoolean(result.Value);
        return !v.HasValue
            ? [ToError(result, invalidBooleanValueError)]
            : NoError;
    }

    /// <summary>
    /// Validates the specified BindEvent&lt;string&gt; object and gets the
    /// tuples representing the error information.
    /// </summary>
    /// <param name="result">
    /// The BindResult&lt;string&gt; object.
    /// </param>
    /// <param name="isValidValue">
    /// The function that returns whether a value of the argument is valid or
    /// not.
    /// </param>
    /// <param name="invalidIntegerValueError">
    /// The error message when it is unable to parse an integer value.
    /// </param>
    /// <param name="invalidValueRangeError">
    /// The error message when the parsed value is invalid.
    /// </param>
    /// <returns>
    /// <see cref="NoError"/> if the specified BindEvent&lt;string&gt; object
    /// can be parsed successfully. Otherwise, the errors.
    /// </returns>
    public static IEnumerable<WhereWhy> ValidateInt(
        BindResult<string>? result,
        Func<int, bool> isValidValue,
        string invalidIntegerValueError,
        string invalidValueRangeError)
    {
        return (result is null)
            ? NoError
            : (ParseInt(result.Value) is not {} v)
            ? [ToError(result, invalidIntegerValueError)]
            : !isValidValue(v)
            ? [ToError(result, invalidValueRangeError)]
            : NoError;
    }

    private static WhereWhy ToError(
            BindResult<string> result,
            string message)
        => new(result.Line, result.Column, $"{message}: '{result.Value}'");

    /// <summary>
    /// Gets the integer value that results from parsing the specified string.
    /// </summary>
    /// <param name="s">
    /// The string representing an integer value.
    /// </param>
    /// <returns>
    /// The tuple of the boolean value representing whether the value has been
    /// parsed successfully and the parsed integer value.
    /// </returns>
    private static int? ParseInt(string s)
        => int.TryParse(s, out var value) ? value : null;

    /// <summary>
    /// Gets the boolean value that results from parsing the specified string.
    /// </summary>
    /// <param name="s">
    /// The string representing a boolean value.
    /// </param>
    /// <returns>
    /// The optional boolean value representing whether the value has been
    /// parsed successfully and the parsed boolean value.
    /// </returns>
    private static bool? ParseBoolean(string s)
        => BooleanMap.TryGetValue(s, out var b) ? b : null;
}
