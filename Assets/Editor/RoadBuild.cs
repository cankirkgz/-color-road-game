using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace RenkYolu.Editor
{
    public static class RoadBuild
    {
        [MenuItem("Renk Yolu/Build Android APK")]
        public static void Android() => Build(BuildTarget.Android, "Builds/RenkYolu.apk");

        [MenuItem("Renk Yolu/Build Windows")]
        public static void Windows() => Build(BuildTarget.StandaloneWindows64, "Builds/Windows/RenkYolu.exe");

        private static void Build(BuildTarget target, string defaultPath)
        {
            string location = Path.GetFullPath(Argument("-roadOutput") ?? defaultPath);
            Directory.CreateDirectory(Path.GetDirectoryName(location));
            string[] scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
            if (scenes.Length == 0 || scenes.Any(scene => !File.Exists(scene)))
                throw new InvalidOperationException("An enabled build scene is missing.");

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = location,
                target = target,
                options = BuildOptions.CompressWithLz4
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Renk Yolu build failed: " + report.summary.result);

            Debug.Log("Renk Yolu build complete: " + location + " (" + report.summary.totalSize + " bytes)");
        }

        private static string Argument(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, name);
            return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
        }
    }
}
