﻿namespace MarkdownSnippets;

public static class GitRepoDirectoryFinder
{
    public static string FindForFilePath([CallerFilePath] string sourceFilePath = "")
    {
        Guard.FileExists(sourceFilePath, nameof(sourceFilePath));
        var directory = Path.GetDirectoryName(sourceFilePath)!;
        return FindForDirectory(directory);
    }

    public static string FindForDirectory(string directory)
    {
        Guard.DirectoryExists(directory, nameof(directory));
        if (TryFind(directory, out var path))
        {
            return path;
        }

        throw new("Could not find git repository directory");
    }

    static bool TryFind(string directory, [NotNullWhen(true)] out string? path)
    {
        if (TryFind(directory, ".git", out var targetDirectory))
        {
            path = targetDirectory;
            return true;
        }

        if (TryFind(directory, ".gitignore", out targetDirectory))
        {
            path = targetDirectory;
            return true;
        }

        path = null;
        return false;
    }

    public static bool IsInGitRepository(string directory)
    {
        Guard.DirectoryExists(directory, nameof(directory));
        return TryFind(directory, out _);
    }

    static bool TryFind(string directory, string suffix, [NotNullWhen(true)] out string? path)
    {
        Guard.DirectoryExists(directory, nameof(directory));

        do
        {
            var combine = Path.Combine(directory, suffix);
            if (Directory.Exists(combine) ||
                File.Exists(combine))
            {
                path = directory;
                return true;
            }

            var parent = Directory.GetParent(directory);
            if (parent == null)
            {
                path = null;
                return false;
            }

            directory = parent.FullName;
        } while (true);
    }
}