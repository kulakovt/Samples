using Build.Common;
using Nuke.Common;

class AppBuild : NukeBuild, IDefaultBuild
{
    public string ServiceName => "Buldac";

    public int Major => 0;

    public int Minor => 42;

    public static int Main()
        => Execute<AppBuild>(x => ((IDefaultBuild)x).Default);
}
