using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class WayfinderBuild
{
    public static void BuildWindows64()
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string outputDirectory = Path.Combine(projectRoot, "Builds", "Windows");
        Directory.CreateDirectory(outputDirectory);

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Scenes/Level01_ForestAdventure.unity" },
            locationPathName = Path.Combine(outputDirectory, "The Paradox.exe"),
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
            throw new BuildFailedException($"Windows build failed: {report.summary.result}");

        Debug.Log($"The Paradox Windows build created at {options.locationPathName}");
    }
}
