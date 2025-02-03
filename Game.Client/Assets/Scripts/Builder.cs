using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Reporting;
using UnityEditor;
using UnityEngine;
using System.Runtime.InteropServices;

public class Builder
{
    // This function will be called from the build process
    public static void Build()
    {


        string[] defaultScene = {
            "Assets/Scenes/Login.unity",
            "Assets/Scenes/Register.unity",
            "Assets/Scenes/World.unity"
            };

        BuildPlayerOptions devBuildPlayerOptions = new BuildPlayerOptions()
        {
            scenes = defaultScene,
            locationPathName = GetArg("-outputPath"),
            target = BuildTarget.StandaloneWindows,
            extraScriptingDefines = new[] { GetArg("-buildEnvironment") },
            options = BuildOptions.None
        };

        BuildPipeline.BuildPlayer(devBuildPlayerOptions);
    }
    public static string GetArg(string name)
    {
        var args = System.Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == name && args.Length > i + 1)
            {
                return args[i + 1];
            }
        }
        return null;
    }

}
