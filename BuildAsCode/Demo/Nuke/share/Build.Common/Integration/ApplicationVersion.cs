namespace Build.Common.Integration;

internal record ApplicationVersion(
    string AssemblyVersion,
    string Version,
    string FileVersion,
    string InformationalVersion);