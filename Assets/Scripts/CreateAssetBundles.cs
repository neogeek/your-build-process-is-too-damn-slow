#if UNITY_EDITOR

using System.IO;
using UnityEditor;

public static class CreateAssetBundles
{

    [MenuItem("Assets/Build AssetBundles")]
    private static void BuildAllAssetBundles()
    {
        var assetBundleDirectory = "Assets/AssetBundles";

        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }

        BuildPipeline.BuildAssetBundles(assetBundleDirectory, BuildAssetBundleOptions.None,
            EditorUserBuildSettings.activeBuildTarget);
    }

}

#endif
