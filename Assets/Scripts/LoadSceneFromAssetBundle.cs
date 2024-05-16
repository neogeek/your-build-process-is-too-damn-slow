using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class LoadSceneFromAssetBundle : MonoBehaviour
{

    [SerializeField]
    private string assetBundleUrl;

    [SerializeField]
    private string scenePath;

    private Scene sceneRef;

    public void HandleButtonClick()
    {

        StartCoroutine(LoadAssetBundle());

    }

    private IEnumerator LoadAssetBundle()
    {

        if (sceneRef.IsValid())
        {

            yield return SceneManager.UnloadSceneAsync(sceneRef);

        }

        using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(assetBundleUrl))
        {

            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {

                Debug.Log(uwr.error);

            }
            else
            {

                AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(uwr);

                if (bundle.isStreamedSceneAssetBundle)
                {

                    yield return SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);

                    sceneRef = SceneManager.GetSceneByPath(scenePath);

                    SceneManager.SetActiveScene(sceneRef);

                }

                bundle.Unload(false);

            }

        }

    }

}
