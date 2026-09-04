#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

namespace Airox.Editor
{
    public static class AiroxAndroidBuild
    {
        [MenuItem("Airox/Build Android APK")]
        public static void BuildApk()
        {
            var output = Path.GetFullPath("Builds/Airox-v9.2.44.apk");
            Directory.CreateDirectory(Path.GetDirectoryName(output)!);
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/Bootstrap.unity", "Assets/Scenes/BR_Prototype.unity" },
                locationPathName = output,
                target = BuildTarget.Android,
                options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded)
                Debug.LogError("[Airox] APK build failed: " + report.summary); return;
            Debug.Log($"[Airox] APK built: {output}");
        }
    }
}
#endif
