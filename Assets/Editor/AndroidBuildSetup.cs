using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class AndroidBuildSetup
{
    private const string SceneFolder = "Assets/Scenes";
    private const string ScenePath = SceneFolder + "/Bootstrap.unity";

    [MenuItem("Tools/Classic Remastered/Prepare Android Build")]
    public static void PrepareAndroidBuild()
    {
        if (!Directory.Exists(SceneFolder))
            Directory.CreateDirectory(SceneFolder);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Bootstrap";
        EditorSceneManager.SaveScene(scene, ScenePath);

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(ScenePath, true)
        };

        PlayerSettings.companyName = "Classic Remastered Studio";
        PlayerSettings.productName = "Free Fire 2017 Classic Remastered";
        PlayerSettings.bundleVersion = "0.1.0";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.classicremastered.freefire2017");
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel23;
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.stripEngineCode = true;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Android build setup complete. Select File > Build Settings > Android, then Build.");
        EditorUtility.DisplayDialog(
            "Android Build Setup Complete",
            "Bootstrap scene created and added to Build Settings. Android is configured for ARM64 and IL2CPP. Install Android Build Support in Unity Hub, then use File > Build Settings > Build.",
            "OK");
    }
}
