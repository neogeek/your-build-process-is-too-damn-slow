using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadFromAssetBundle : MonoBehaviour
{

    private async void Start()
    {
        var assetBundleDirectory = Path.Combine(Application.persistentDataPath, "AssetBundles");

        await Task.WhenAll(new List<Task>()
        {
            AssetBundleUtilities.DownloadAssetBundle(assetBundleDirectory,
                "http://localhost:8000/samplescene",
                progress => Debug.Log($"Downloading: {progress * 100}%"),
                error => Debug.LogError($"Error: {error.Message}")),
            AssetBundleUtilities.DownloadAssetBundle(assetBundleDirectory,
                "http://localhost:8000/prefabs",
                progress => Debug.Log($"Downloading: {progress * 100}%"),
                error => Debug.LogError($"Error: {error.Message}"))
        });
    }

    public void HandleLoadSceneButtonClick()
    {
        var assetBundleDirectory = Path.Combine(Application.persistentDataPath, "AssetBundles");

        var bundleUrl = "http://localhost:8000/samplescene";

        var scenePath = "Assets/Scenes/SceneToLoadViaURL.unity";

        StartCoroutine(AssetBundleUtilities.LoadSceneFromAssetBundle(
            assetBundleDirectory,
            Path.GetFileName(bundleUrl),
            scenePath, LoadSceneMode.Additive,
            progress => Debug.Log($"Loading: {progress * 100}%"),
            error => Debug.LogError($"Error: {error.Message}")));
    }

    public void HandleLoadAssetButtonClick()
    {
        var assetBundleDirectory = Path.Combine(Application.persistentDataPath, "AssetBundles");

        var bundleUrl = "http://localhost:8000/prefabs";

        var assetPath = "Assets/Prefabs/Cube.prefab";

        StartCoroutine(AssetBundleUtilities.LoadAssetFromAssetBundle<GameObject>(
            assetBundleDirectory,
            Path.GetFileName(bundleUrl), assetPath,
            progress => Debug.Log($"Loading: {progress * 100}%"),
            prefab => Instantiate(prefab, Vector3.zero, Quaternion.identity),
            error => Debug.LogError($"Error: {error.Message}")));
    }

}
