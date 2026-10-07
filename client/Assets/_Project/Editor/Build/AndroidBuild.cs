using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace PlowParty.Editor.Build
{
    public static class AndroidBuild
    {
        public const string OutputPath = "Builds/Android/PlowParty.apk";

        private const string PackageId = "com.alexandrkutsar.plowparty";
        private const string ProductName = "Plow Party";
        private const string CompanyName = "Alexandr Kutsar";
        private const AndroidSdkVersions MinSdk = AndroidSdkVersions.AndroidApiLevel26;

        [MenuItem("Plow Party/Build/Apply Android Player Settings")]
        public static void ApplyPlayerSettings()
        {
            PlayerSettings.companyName = CompanyName;
            PlayerSettings.productName = ProductName;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, PackageId);
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = MinSdk;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Plow Party/Build/Android Development APK")]
        public static void BuildDevelopmentApk()
        {
            Build();
        }

        public static void BuildDevelopmentApkFromCommandLine()
        {
            var summary = Build();
            EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
        }

        private static BuildSummary Build()
        {
            ApplyPlayerSettings();
            EditorUserBuildSettings.buildAppBundle = false;
            Directory.CreateDirectory(Path.GetDirectoryName(OutputPath) ?? throw new InvalidOperationException(OutputPath));
            var options = new BuildPlayerOptions
            {
                scenes = EditorBuildSettingsScene.GetActiveSceneList(EditorBuildSettings.scenes),
                locationPathName = OutputPath,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.Development,
            };
            var summary = BuildPipeline.BuildPlayer(options).summary;
            Debug.Log($"Android build {summary.result}: {summary.outputPath}, {summary.totalSize} bytes, {summary.totalErrors} errors, {summary.totalTime}");
            return summary;
        }
    }
}
