using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public static class AssetBundleUtilities
{

    public static async Task DownloadAssetBundle(string assetBundleDirectory, string assetBundleUrl,
        Action<float> progressCallback, Action<Exception> errorCallback)
    {
        // Create the asset bundle directory if it doesn't exist.
        if (!Directory.Exists(assetBundleDirectory))
        {
            Directory.CreateDirectory(assetBundleDirectory);
        }

        // Combine your asset bundle directory with the file name of the asset bundle you are downloading.
        var bundlePath = Path.Combine(assetBundleDirectory, Path.GetFileName(assetBundleUrl));

        // If the asset bundle already exists, return. This prevents re-downloading the same
        // asset bundle more than once.
        if (File.Exists(bundlePath))
        {
            return;
        }

        // Create a request to download the asset bundle.
        using var request = new UnityWebRequest(assetBundleUrl, UnityWebRequest.kHttpVerbGET);

        // Attach a download handler to the request. This will automatically save the file if the
        // download is successful.
        request.downloadHandler = new DownloadHandlerFile(bundlePath) { removeFileOnAbort = true };

        // Start the request for the asset bundle.
        var operation = request.SendWebRequest();

        // Report the progress using the progressCallback method until the operation is completed.
        while (!operation.isDone)
        {
            progressCallback(operation.progress);

            await Task.Yield();
        }

        if (request.result != UnityWebRequest.Result.Success)
        {
            // If not successful, the errorCallback method is called with a WebException.
            errorCallback(new WebException($"Failed to download asset bundle: {request.error}"));
        }
        else
        {
            // If successful, the progressCallback method is called with the final progress.
            progressCallback(operation.progress);
        }
    }

    public static IEnumerator LoadAssetBundle(string assetBundleDirectory, string bundleName,
        Action<float> progressCallback, Action<AssetBundle> loadedCallback, Action<Exception> errorCallback)
    {
        // Combine the asset bundle directory with the name of the asset bundle you are loading.
        var bundlePath = Path.Combine(assetBundleDirectory, bundleName);

        // If the asset bundle doesn't exist, throw an error.
        if (!File.Exists(bundlePath))
        {
            errorCallback(new FileNotFoundException($"Failed to load asset bundle: {bundleName}"));

            yield break;
        }

        // Create a request to load the asset bundle.
        var bundleLoadRequest = AssetBundle.LoadFromFileAsync(bundlePath);

        // Report the progress using the progressCallback method until the operation is completed.
        while (!bundleLoadRequest.isDone)
        {
            progressCallback(bundleLoadRequest.progress);

            yield return null;
        }

        var assetBundle = bundleLoadRequest.assetBundle;

        if (!assetBundle)
        {
            assetBundle.Unload(false);

            // If not successful, the errorCallback method is called with a FileNotFoundException.
            errorCallback(new FileNotFoundException($"Failed to load asset bundle: {bundleName}"));
        }
        else
        {
            // If successful, the progressCallback method is called with the final progress.
            progressCallback(bundleLoadRequest.progress);

            // Finally, call the loadedCallback method with the loaded asset bundle resource.
            loadedCallback(assetBundle);
        }
    }

    public static IEnumerator LoadSceneFromAssetBundle(string assetBundleDir, string bundleName, string scenePath,
        LoadSceneMode loadSceneMode, Action<float> progressCallback, Action<Exception> errorCallback)
    {
        // Create a reference for the asset bundle.
        AssetBundle assetBundle = null;

        // Load the asset bundle and use the loadedCallback to store the reference in the local variable above.
        yield return LoadAssetBundle(assetBundleDir, bundleName, progressCallback,
            bundle => assetBundle = bundle, errorCallback);

        // If not successful, the errorCallback method is called with a FileNotFoundException.
        if (assetBundle == null)
        {
            errorCallback(new FileNotFoundException($"Failed to load asset bundle: {bundleName}"));

            yield break;
        }

        // If the asset bundle does not contain scenes, the errorCallback method is called with an
        // InvalidOperationException.
        if (!assetBundle.isStreamedSceneAssetBundle)
        {
            assetBundle.Unload(false);

            errorCallback(new InvalidOperationException("Can only load a scene using this method."));

            yield break;
        }

        // If the requested scene is not found, the errorCallback method is called with a FileNotFoundException.
        if (Array.Find(assetBundle.GetAllScenePaths(), path => path.Equals(scenePath)) == null)
        {
            assetBundle.Unload(false);

            errorCallback(new FileNotFoundException($"Scene {scenePath} not found in asset bundle {bundleName}."));

            yield break;
        }

        // If successful, the scene is loaded using the LoadSceneMode (Single or Additive)
        var sceneLoadRequest = SceneManager.LoadSceneAsync(scenePath, loadSceneMode);

        yield return sceneLoadRequest;

        assetBundle.Unload(false);
    }

    public static IEnumerator LoadAssetFromAssetBundle<T>(string assetBundleDir, string bundleName, string assetPath,
        Action<float> progressCallback,
        Action<T> instantiateCallback, Action<Exception> errorCallback)
        where T : UnityEngine.Object
    {
        // Create a reference for the asset bundle.
        AssetBundle assetBundle = null;

        // Load the asset bundle and use the loadedCallback to store the reference in the local variable above.
        yield return LoadAssetBundle(assetBundleDir, bundleName, progressCallback,
            bundle => assetBundle = bundle, errorCallback);

        // If not successful, the errorCallback method is called with a FileNotFoundException.
        if (assetBundle == null)
        {
            errorCallback(new FileNotFoundException($"Failed to load asset bundle: {bundleName}"));

            yield break;
        }

        // If the asset bundle contains scenes, the errorCallback method is called with an InvalidOperationException.
        if (assetBundle.isStreamedSceneAssetBundle)
        {
            assetBundle.Unload(false);

            errorCallback(new InvalidOperationException("Can not load a scene using this method."));

            yield break;
        }

        // Load the requested asset using the given path.
        var assetLoadRequest = assetBundle.LoadAssetAsync<T>(assetPath);

        yield return assetLoadRequest;

        if (assetLoadRequest.asset == null)
        {
            // If not successful, the errorCallback method is called with a FileNotFoundException.
            errorCallback(new FileNotFoundException($"Asset {assetPath} not found in asset bundle {bundleName}."));
        }
        else
        {
            // If successful, the instantiateCallback method is called with the asset type-casted to the requested type.
            instantiateCallback(assetLoadRequest.asset as T);
        }

        assetBundle.Unload(false);
    }

}
