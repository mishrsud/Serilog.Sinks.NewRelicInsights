using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.BuildServers;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main () => Execute<Build>(x => x.Publish);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;
    
    readonly string ApiKey = Variable("api_key");

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion] readonly GitVersion GitVersion;

    string Source => "https://api.nuget.org/v3/index.json";
    
    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath ArtifactsDirectory => RootDirectory / "artifacts";

    AbsolutePath ProjectToPackDirectory => RootDirectory / "src" / "Serilog.Sinks.NewRelicInsights" / "Serilog.Sinks.NewRelicInsights.csproj";

    Target Clean => _ => _
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(ArtifactsDirectory);
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.GetNormalizedAssemblyVersion())
                .SetFileVersion(GitVersion.GetNormalizedFileVersion())
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .EnableNoRestore());
        });

    Target Pack => _ => _
        .Description("Packages artifacts to nuget")
        .DependsOn(Compile)
        .Executes(() =>
        {
            Console.Out.WriteLine($"Setting version: {GitVersion.NuGetVersionV2}");
            DotNetPack(settings => settings
                .SetProject(ProjectToPackDirectory)
                .SetConfiguration(Configuration)
                .SetNoBuild(true)
                .SetNoRestore(true)
                .SetOutputDirectory(ArtifactsDirectory)
                .EnableIncludeSource()
                .SetVersion(GitVersion.NuGetVersionV2));
        });

    Target Publish => _ => _
        .Description("Publish Nuget Package")
        .After(Pack)
        .DependsOn(Pack)
        .OnlyWhenDynamic(() => !string.IsNullOrWhiteSpace(ApiKey))
        .Executes(() =>
        {
            GlobFiles(ArtifactsDirectory, "*.nupkg").NotEmpty()
                .ForEach(pkg => DotNetNuGetPush(settings => settings
                    .SetTargetPath(pkg)
                    .SetSource(Source)
                    .SetApiKey(ApiKey)));
        });
}
