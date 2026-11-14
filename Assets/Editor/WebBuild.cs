using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Remus > Build for itch.io (Web): builds the game for the browser into Builds/Web and zips it into
// Builds/Remus-Web.zip, ready to upload to itch.io as an HTML game.
// Needs "Web Build Support" installed for this Unity version (Unity Hub > Installs > Add modules).
// On itch.io: Kind of project = HTML, tick "This file will be played in the browser", set the
// embed size to 1280 x 720 (or any 16:9 size) and tick "Fullscreen button". The page template
// (Assets/WebGLTemplates/Remus) keeps the game 16:9 and fitted to whatever size the frame is.
public static class WebBuild
{
    private const string OutputFolder = "Builds/Web";
    private const string ZipPath = "Builds/Remus-Web.zip";

    [MenuItem("Remus/Build for itch.io (Web)")]
    public static void BuildForItch()
    {
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL))
        {
            Report("Web Build Support isn't installed for this Unity version. Add it in Unity Hub > Installs > (this version) > Add modules.", false);
            return;
        }

        ApplySettings();

        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = EnabledScenes(),
            locationPathName = OutputFolder,
            target = BuildTarget.WebGL,
            options = BuildOptions.None,
        });

        if (report.summary.result != BuildResult.Succeeded)
        {
            Report($"Web build failed: {report.summary.totalErrors} error(s). See the Console.", false);
            return;
        }

        if (File.Exists(ZipPath)) File.Delete(ZipPath);
        ZipFile.CreateFromDirectory(OutputFolder, ZipPath);
        Report($"Web build ready: {ZipPath} ({new FileInfo(ZipPath).Length / (1024 * 1024)} MB). Upload that zip to itch.io as an HTML game.", true);
    }

    // The Player Settings the web build needs (also set once by hand in Project Settings > Player > Web)
    public static void ApplySettings()
    {
        PlayerSettings.WebGL.template = "PROJECT:Remus";
        // itch.io doesn't send the headers for pre-compressed files, so the loader unpacks them itself
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.defaultWebScreenWidth = 1280;
        PlayerSettings.defaultWebScreenHeight = 720;
        PlayerSettings.runInBackground = false;
    }

    private static string[] EnabledScenes()
    {
        var scenes = new System.Collections.Generic.List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            if (scene.enabled) scenes.Add(scene.path);
        return scenes.ToArray();
    }

    private static void Report(string message, bool success)
    {
        if (success) Debug.Log(message); else Debug.LogError(message);
        if (!Application.isBatchMode) EditorUtility.DisplayDialog("Build for itch.io", message, "OK");
    }
}
