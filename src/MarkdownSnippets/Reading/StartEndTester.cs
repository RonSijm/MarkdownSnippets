﻿using System.Diagnostics.CodeAnalysis;

public delegate bool EndFunc(string line);

static class StartEndTester
{
    internal static bool IsStartOrEnd(string trimmedLine) =>
        IsBeginSnippet(trimmedLine) ||
        IsEndSnippet(trimmedLine) ||
        IsStartRegion(trimmedLine) ||
        IsEndRegion(trimmedLine);

    internal static bool IsStart(
        string trimmedLine,
        string path,
        [NotNullWhen(true)] out string? currentKey,
        [NotNullWhen(true)] out EndFunc? endFunc)
    {
        if (IsBeginSnippet(trimmedLine, path, out currentKey))
        {
            endFunc = IsEndSnippet;
            return true;
        }

        if (IsStartRegion(trimmedLine, out currentKey))
        {
            endFunc = IsEndRegion;
            return true;
        }

        endFunc = throwFunc;
        return false;
    }

    static EndFunc throwFunc = _ => throw new("Do not use out func");

    static bool IsEndRegion(string line) =>
        line.StartsWith("#endregion", StringComparison.Ordinal);

    static bool IsEndSnippet(string line) =>
        IndexOf(line, "end-snippet") >= 0;

    static bool IsStartRegion(string line) =>
        line.StartsWith("#region ", StringComparison.Ordinal);

    internal static bool IsStartRegion(
        string line,
        [NotNullWhen(true)] out string? key)
    {
        if (!line.StartsWith("#region ", StringComparison.Ordinal))
        {
            key = null;
            return false;
        }

        var substring = line[8..].Trim();

        if (substring.Contains(' '))
        {
            key = null;
            return false;
        }

        if (!KeyValidator.IsValidKey(substring.AsSpan()))
        {
            key = null;
            return false;
        }

        key = substring;
        return true;
    }

    static bool IsBeginSnippet(string line)
    {
        var startIndex = IndexOf(line, "begin-snippet: ");
        return startIndex != -1;
    }

    internal static bool IsBeginSnippet(
        string line,
        string path,
        [NotNullWhen(true)] out string? key)
    {
        var beginSnippetIndex = IndexOf(line, "begin-snippet: ");
        if (beginSnippetIndex == -1)
        {
            key = null;
            return false;
        }

        var startIndex = beginSnippetIndex + 15;
        var substring = line
            .TrimBackCommentChars(startIndex);
        var split = substring.SplitBySpace();
        if (split.Length == 0)
        {
            throw new SnippetReadingException(
                $"""
                 No Key could be derived.
                 Path: {path}
                 Line: '{line}'
                 """);
        }

        key = split[0];
        if (split.Length != 1)
        {
            throw new SnippetReadingException(
                $"""
                 Too many parts.
                 Path: {path}
                 Line: '{line}'
                 """);
        }

        if (!KeyValidator.IsValidKey(key.AsSpan()))
        {
            throw new SnippetReadingException(
                $"""
                 Key cannot contain whitespace or start/end with symbols.
                 Key: {key}
                 Path: {path}
                 Line: {line}
                 """);
        }

        return true;
    }

    static int IndexOf(string line, string value)
    {
        if (value.Length > line.Length)
        {
            return -1;
        }

        var charactersToScan = Math.Min(line.Length, value.Length + 10);
        return line.IndexOf(value, startIndex: 0, count: charactersToScan, StringComparison.Ordinal);
    }
}