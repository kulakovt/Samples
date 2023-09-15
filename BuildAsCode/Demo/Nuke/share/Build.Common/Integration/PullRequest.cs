namespace Build.Common.Integration;

internal record PullRequest(
    string Number,
    string SourceBranchName);