using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace LearnAIGame.EditorTools
{
    public static class LastTrainBuild
    {
        [MenuItem("LearnAIGame/Build/Export iOS Xcode project")]
        public static void ExportIOS()
        {
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.iOS, BuildTarget.iOS))
                throw new BuildFailedException("This Editor cannot load iOS Build Support. Repair/add it in Unity Hub before exporting.");
            string bundleId = Environment.GetEnvironmentVariable("LASTTRAIN_BUNDLE_ID");
            string build = Environment.GetEnvironmentVariable("LASTTRAIN_BUILD_NUMBER");
            if (string.IsNullOrEmpty(bundleId) || !Regex.IsMatch(bundleId, @"^[A-Za-z0-9-]+(\.[A-Za-z0-9-]+){2,}$"))
                throw new BuildFailedException("Set LASTTRAIN_BUNDLE_ID to your registered Apple bundle ID. No placeholder ID is used for distribution.");
            if (!int.TryParse(build, out int number) || number < 1)
                throw new BuildFailedException("Set LASTTRAIN_BUILD_NUMBER to a positive, unused App Store Connect build number.");
            var campaign = JsonUtility.FromJson<LearnAIGame.Story.StoryCampaign>(
                File.ReadAllText("Assets/Resources/last_train.json"));
            if (campaign.chapters.Length == 0) throw new BuildFailedException("Campaign has no chapters.");
            foreach (var chapter in campaign.chapters)
                foreach (var beat in chapter.beats)
                    if (!File.Exists("Assets/Resources/" + beat.artResource + ".png"))
                        throw new BuildFailedException("Missing illustration: " + beat.artResource);
            var icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/LastTrainIcon.png");
            if (icon == null) throw new BuildFailedException("Missing app icon.");
            PlayerSettings.SetIcons(NamedBuildTarget.Unknown, new[] { icon }, IconKind.Any);
            PlayerSettings.productName = "Last Train";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, bundleId);
            PlayerSettings.bundleVersion = "0.2.0";
            PlayerSettings.iOS.buildNumber = build;
            PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
            PlayerSettings.iOS.targetOSVersionString = "15.0";
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetArchitecture(NamedBuildTarget.iOS, 1); // ARM64
            string team = Environment.GetEnvironmentVariable("LASTTRAIN_APPLE_TEAM_ID");
            PlayerSettings.iOS.appleDeveloperTeamID = team ?? "";
            PlayerSettings.iOS.appleEnableAutomaticSigning = !string.IsNullOrEmpty(team);
            string output = Path.GetFullPath("Builds/iOS");
            if (Directory.Exists(output))
                throw new BuildFailedException("Builds/iOS already exists. Move the previous export aside before producing a fresh export.");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/Bootstrap.unity" },
                locationPathName = output,
                target = BuildTarget.iOS,
                options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException("iOS export failed: " + report.summary.result);
            Debug.Log("iOS Xcode project exported to " + output + ". Archive/sign in Xcode; this command does not upload to Apple.");
        }
    }
}
