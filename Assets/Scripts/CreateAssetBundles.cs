#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEngine;

public static class CreateAssetBundles
{

    [MenuItem("Assets/Build AssetBundles")]
    private static void BuildAllAssetBundles()
    {
        var assetBundleDirectory =
            Path.Combine("Assets/AssetBundles", EditorUserBuildSettings.activeBuildTarget.ToString());

        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }

        BuildPipeline.BuildAssetBundles(
            assetBundleDirectory,
            BuildAssetBundleOptions.None,
            EditorUserBuildSettings.activeBuildTarget);
    }

}

#endif
