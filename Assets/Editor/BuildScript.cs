using UnityEditor;
using UnityEngine;

public class BuildScript
{
    public static void BuildWindows()
    {
        string[] scenes = { "Assets/Scenes/TestScene.unity" };
        string buildPath = "C:/Users/Zbook de Mario/AppData/Local/Temp/opencode/build_win3";
        
        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildPath + "/MissionSimulator.exe",
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };
        
        var report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + report.summary.totalSize + " bytes");
        }
        else
        {
            Debug.LogError("Build failed: " + report.summary.result);
        }
    }
}