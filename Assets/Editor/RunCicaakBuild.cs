using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class RunCicaakBuild
{
    private const string MenuScenePath = "Assets/Scenes/MainMenu.unity";
    private const string GameScenePath = "Assets/Scenes/Game.unity";

    [MenuItem("RUN CICAAK/Generate Scenes")]
    public static void GenerateScenes()
    {
        Directory.CreateDirectory("Assets/Scenes");

        Scene gameScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject bootstrap = new GameObject("RUN CICAAK Prototype Bootstrap");
        PrototypeHouseBootstrap house = bootstrap.AddComponent<PrototypeHouseBootstrap>();
        house.buildOnStart = true;
        EditorSceneManager.SaveScene(gameScene, GameScenePath);

        Scene menuScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject menuRoot = new GameObject("RUN CICAAK Main Menu");
        menuRoot.AddComponent<MainMenuController>();
        menuRoot.AddComponent<MainMenuUI>();
        EditorSceneManager.SaveScene(menuScene, MenuScenePath);

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(MenuScenePath, true),
            new EditorBuildSettingsScene(GameScenePath, true)
        };

        AssetDatabase.SaveAssets();
        Debug.Log("RUN CICAAK scenes generated and added to Build Settings.");
    }

    [MenuItem("RUN CICAAK/Build Android APK")]
    public static void BuildAndroid()
    {
        GenerateScenes();

        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
                throw new Exception("Could not switch Unity build target to Android.");
        }

        PlayerSettings.productName = "RUN CICAAK";
        PlayerSettings.companyName = "ZORA";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.zora.runcicaak");
        PlayerSettings.bundleVersion = "1.0.0";
        PlayerSettings.Android.bundleVersionCode = 1;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        EditorUserBuildSettings.buildAppBundle = false;

        string outputPath = Environment.GetEnvironmentVariable("RUN_CICAAK_BUILD_PATH");
        if (string.IsNullOrEmpty(outputPath)) outputPath = "Build/RUN-CICAAK.apk";
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = new[] { MenuScenePath, GameScenePath },
            locationPathName = outputPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
            throw new Exception("Android build failed: " + report.summary.result + " | " + report.summary.totalErrors + " errors");

        Debug.Log("RUN CICAAK APK created: " + outputPath);
    }
}