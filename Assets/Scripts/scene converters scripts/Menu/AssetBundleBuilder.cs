
// system
using System.IO;

// unity
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
//using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Content;

////
//// Résumé :
////     Build assetBundle without any special option.
//None = 0,
////
//// Résumé :
////     Don't compress the data when creating the asset bundle.
//UncompressedAssetBundle = 1,
////
//// Résumé :
////     Includes all dependencies.
//CollectDependencies = 2,
////
//// Résumé :
////     Forces inclusion of the entire asset.
//CompleteAssets = 4,
////
//// Résumé :
////     Do not include type information within the AssetBundle.
//DisableWriteTypeTree = 8,
////
//// Résumé :
////     Builds an asset bundle using a hash for the id of the object stored in the asset
////     bundle.
//DeterministicAssetBundle = 16,
////
//// Résumé :
////     Force rebuild the assetBundles.
//ForceRebuildAssetBundle = 32,
////
//// Résumé :
////     Ignore the type tree changes when doing the incremental build check.
//IgnoreTypeTreeChanges = 64,
////
//// Résumé :
////     Append the hash to the assetBundle name.
//AppendHashToAssetBundleName = 128,
////
//// Résumé :
////     Use chunk-based LZ4 compression when creating the AssetBundle.
//ChunkBasedCompression = 256,
////
//// Résumé :
////     Do not allow the build to succeed if any errors are reporting during it.
//StrictMode = 512,
////
//// Résumé :
////     Do a dry run build.
//DryRunBuild = 1024,
////
//// Résumé :
////     Disables Asset Bundle LoadAsset by file name.
//DisableLoadAssetByFileName = 4096,
////
//// Résumé :
////     Disables Asset Bundle LoadAsset by file name with extension.
//DisableLoadAssetByFileNameWithExtension = 8192,
////
//// Résumé :
////     Removes the Unity Version number in the Archive File & Serialized File headers
////     during the build.
//AssetBundleStripUnityVersion = 32768


public class AssetBundleSettingsWindow : EditorWindow {
    
    public string path;
    string assetBundleName = "default";

    void OnGUI() {

        GUILayout.Label(string.Format("Prefab {0} asset bundle settings", path), EditorStyles.boldLabel);
        assetBundleName = EditorGUILayout.TextField("Assetbundle name", assetBundleName);

        if (GUILayout.Button("Apply")) {
            Debug.Log("APPLY: " + path + " " + assetBundleName);
            AssetImporter.GetAtPath(path).SetAssetBundleNameAndVariant(assetBundleName, "");
            Close();
        }

    }
}

public class AssetBundleBuilder : MonoBehaviour{

    private static readonly string assetBundleDirectory = "./assetsBundles";
    
    [MenuItem("AssetBundles/Choose bundle name for current selection")]
    static void ChangeBundleName() {
        AssetBundleSettingsWindow window = (AssetBundleSettingsWindow)EditorWindow.GetWindow(typeof(AssetBundleSettingsWindow));
        window.path = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
        window.Show();
    }

    [MenuItem("AssetBundles/Remove unused asset bundles names")]
    static void RemoveUnusedAssetBundleNames() {
        AssetDatabase.RemoveUnusedAssetBundleNames();
    }


    [MenuItem("AssetBundles/Build AssetBundles")]
    static void BuildAllAssetBundles() {
        BuildAllAssetBundles(BuildAssetBundleOptions.None);
    }

    [MenuItem("AssetBundles/Force rebuild AssetBundles")]
    static void ForceRebuildAllAssetBundles() {
        BuildAllAssetBundles(BuildAssetBundleOptions.ForceRebuildAssetBundle);
    }

    [MenuItem("AssetBundles/Clear All AssetBundles")]
    static void ClearAllAssetBundles()
    {
        var assetBundleDependencies = AssetDatabase.GetAllAssetBundleNames();

        foreach (var bundleName in assetBundleDependencies)
        {
            AssetDatabase.RemoveAssetBundleName(bundleName, true);
            Debug.Log("Gone good for " + bundleName);
        }


        /*foreach (string prefabPath in AssetDatabase.FindAssets("t:prefab", new string[] { "Assets/Prefabs" }))
        {
            try {
                
                Debug.Log(AssetDatabase.GUIDToAssetPath(prefabPath));
                var assetBundleName = AssetDatabase.GetImplicitAssetBundleName(AssetDatabase.GUIDToAssetPath(prefabPath));
                var assetBundleDependencies = AssetDatabase.GetAssetBundleDependencies(assetBundleName, true);

                AssetDatabase.RemoveAssetBundleName(assetBundleName, true);

                foreach (var bundleName in assetBundleDependencies)
                {
                    AssetDatabase.RemoveAssetBundleName(bundleName, true);
                }
                Debug.Log("Gone good for " + assetBundleName);
            }
            catch
            {

            }
        }*/

    }


    [MenuItem("AssetBundles/Clean AssetBundles dir")]
    static void CleanAllAssetBundles() {

        Debug.Log("Start cleaning directory: " + assetBundleDirectory);
        string[] files = Directory.GetFiles(assetBundleDirectory);
        foreach (var file in files) {
            Debug.Log("Delete: " + file);
            File.Delete(file);
        }
    }

    static void BuildAllAssetBundles(BuildAssetBundleOptions options) {

        Debug.Log("Start generating assets bundles to: " + assetBundleDirectory);
        if (!Directory.Exists(assetBundleDirectory)) {
            Debug.Log("Directory \"" + assetBundleDirectory + "\" is created.");
            Directory.CreateDirectory(assetBundleDirectory);
        }

        
        Debug.Log("Asset bundle build list:");
        var manifest = BuildPipeline.BuildAssetBundles(assetBundleDirectory, options, EditorUserBuildSettings.activeBuildTarget);
        if (manifest != null) {
            foreach (var ab in manifest.GetAllAssetBundles()) {
                Debug.Log(ab);
            }
        } else {
            Debug.Log("Manifest is null.");
        }

        string[] files = Directory.GetFiles(assetBundleDirectory);
        foreach (var file in files) {

            if(file == "assetsbundles") {
                continue;
            }

            if (file.Contains(".manifest")) {   // do not touch manifest files
                continue;
            }

            if (file.Contains(".unity3d")) {    // do not touch other unity3d files
                continue;
            }

            // rename bundle file into bundle.unity3d
            string newName = file + ".unity3d";
            if (File.Exists(newName)) {
                File.Delete(newName);
            }
            File.Move(file, newName);
        }
        Debug.Log("End building asset bundles.");
    }

    //public static class BuildAssetBundlesExample {
    //    public static bool BuildAssetBundles(string outputPath, bool forceRebuild, bool useChunkBasedCompression, BuildTarget buildTarget) {

    //        var options = BuildAssetBundleOptions.None;
    //        if (useChunkBasedCompression) {
    //            options |= BuildAssetBundleOptions.ChunkBasedCompression;// Use chunk-based LZ4 compression when creating the AssetBundle.
    //        }

    //        if (forceRebuild) {
    //            options |= BuildAssetBundleOptions.ForceRebuildAssetBundle; // This allows you to rebuild the assetBundle even if none of the included assets have changed.
    //        }


    //        //// Get the set of bundle to build
    //        var bundles = ContentBuildInterface.GenerateAssetBundleBuilds();

    //        //// Update the addressableNames to load by the file name without extension
    //        //for (var i = 0; i < bundles.Length; i++)
    //        //    bundles[i].addressableNames = bundles[i].assetNames.Select(Path.GetFileNameWithoutExtension).ToArray();

    //        //var manifest = CompatibilityBuildPipeline.BuildAssetBundles(m_Settings.outputPath, bundles, options, m_Settings.buildTarget);


    //        // public static CompatibilityAssetBundleManifest BuildAssetBundles(string outputPath, BuildAssetBundleOptions assetBundleOptions, BuildTarget targetPlatform);
    //        // public static CompatibilityAssetBundleManifest BuildAssetBundles(string outputPath, AssetBundleBuild[] builds, BuildAssetBundleOptions assetBundleOptions, BuildTarget targetPlatform);

    //        Directory.CreateDirectory(outputPath);
    //        var manifest = CompatibilityBuildPipeline.BuildAssetBundles(outputPath, options, buildTarget);
    //        return manifest != null;
    //    }
    //}

}
#endif