using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Build.Common.Integration;

internal static class BaseBuildExtensions
{
    private static readonly Regex PreReleaseSuffixEscaper = new("[^0-9A-Za-z-]", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static T GetValue<T>(this IBaseBuild build, Expression<Func<T>> parameterExpression)
        where T : class
        => build.TryGetValue(parameterExpression)
           ?? throw new InvalidOperationException($"Cannot get value for {parameterExpression}");

    public static PullRequest? ResolvePullRequest(this IBaseBuild build)
    {
        var number = build.PullRequestNumber;
        var branch = build.PullRequestSourceBranch;
        
        // TeamCity can't replace parameters if they don't exist
        if (number.Contains('%'))
        {
            // This is not a pull request
            return default;
        }

        // Sometimes TeamCity can't provide the name of the source branch
        if (branch.Contains('%'))
        {
            branch = "merge";
        }

        return new(number, branch);
    }
    
    public static string FormatPreReleaseSuffix(this IBaseBuild build)
    {
        var name = build.Branch;
        if (name == "master")
        {
            // Pre-release suffix not required
            return String.Empty;
        }

        var pullRequest = build.ResolvePullRequest();
        if (pullRequest != null)
        {
            name = $"{pullRequest.SourceBranchName}-pr{pullRequest.Number}";
        }

        var suffix = PreReleaseSuffixEscaper.Replace(name, "-");
        if (suffix.Length == 0 || !Char.IsLetter(suffix[0]))
        {
            suffix = "pre" + suffix;
        }

        return $"-{suffix}";
    }

    public static ApplicationVersion ResolveVersion(this IBaseBuild build)
    {
        var major = build.Major;
        var minor = build.Minor;
        var patch = build.BuildCounter;
        var preReleaseSuffix = build.FormatPreReleaseSuffix();

        return new(
            AssemblyVersion: $"{major}.{minor}",
            Version: $"{major}.{minor}.{patch}{preReleaseSuffix}",
            FileVersion: $"{major}.{minor}.{patch}",
            InformationalVersion: $"{major}.{minor}.{patch}{preReleaseSuffix}+commit.${build.VcsNumber}");
    }
}
