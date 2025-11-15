using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

/// <summary>
/// 간단하지만 실무에서 바로 쓸 수 있는 AssetBundle 로더 유틸리티
/// 사용 방식:
/// 1) 로컬 파일에서 빠르게 로드: LoadFromFileAsync(path, onLoaded...)
/// 2) 원격 URL에서 비동기 다운로드: LoadFromUrlAsync(url, onLoaded...)
/// 3) 번들에서 에셋 로드: LoadAssetAsync<T>(bundle, assetName, onLoaded...)
/// 4) 번들에서 씬 로드: LoadSceneFromBundleAsync(bundle, sceneName, LoadSceneMode.Additive, ...)
///
/// 특징:
/// - 에러 처리와 진행도 콜백 제공
/// - 로컬 파일은 AssetBundle.LoadFromFileAsync 사용 (빠름)
/// - 원격은 UnityWebRequest + DownloadHandlerAssetBundle 사용
/// - 필요시 캐시 체크/사용 가능 (GetAssetBundle overload의 version/hash 활용을 권장)
/// </summary>
public class AssetBundleLoader : MonoBehaviour
{
    // 싱글턴 스타일 접근을 편하게 하기 위함 (원하면 제거)
    public static AssetBundleLoader Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    #region Load From Local File
    /// <summary>
    /// 로컬 파일에서 AssetBundle을 비동기 로드합니다. (예: Application.streamingAssetsPath + "/mybundle")
    /// </summary>
    public Coroutine LoadFromFileAsync(string filePath, Action<AssetBundle> onLoaded, Action<float> onProgress = null, Action<string> onError = null)
    {
        return StartCoroutine(LoadFromFileCoroutine(filePath, onLoaded, onProgress, onError));
    }

    private IEnumerator LoadFromFileCoroutine(string filePath, Action<AssetBundle> onLoaded, Action<float> onProgress, Action<string> onError)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            onError?.Invoke("filePath is null or empty");
            yield break;
        }

        // 로컬 파일 로드는 AssetBundle.LoadFromFileAsync가 더 빠름
        var asyncOp = AssetBundle.LoadFromFileAsync(filePath);
        if (asyncOp == null)
        {
            onError?.Invoke($"Failed to start loading from file: {filePath}");
            yield break;
        }

        while (!asyncOp.isDone)
        {
            onProgress?.Invoke(asyncOp.progress);
            yield return null;
        }

        var bundle = asyncOp.assetBundle;
        if (bundle == null)
        {
            onError?.Invoke($"Loaded AssetBundle is null (path: {filePath})");
            yield break;
        }

        onLoaded?.Invoke(bundle);
    }
    #endregion

    #region Load From URL (UnityWebRequest)
    /// <summary>
    /// 원격 URL에서 AssetBundle을 비동기로 다운로드합니다.
    /// version이나 hash를 넣어 캐싱을 활용할 수 있습니다 (선택).
    /// </summary>
    public Coroutine LoadFromUrlAsync(string url, Action<AssetBundle> onLoaded, Action<float> onProgress = null, Action<string> onError = null, uint crc = 0)
    {
        return StartCoroutine(LoadFromUrlCoroutine(url, onLoaded, onProgress, onError, crc));
    }

    private IEnumerator LoadFromUrlCoroutine(string url, Action<AssetBundle> onLoaded, Action<float> onProgress, Action<string> onError, uint crc)
    {
        if (string.IsNullOrEmpty(url))
        {
            onError?.Invoke("url is null or empty");
            yield break;
        }

        using (var uwr = UnityWebRequestAssetBundle.GetAssetBundle(url, crc))
        {
            var request = uwr.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            while (!request.isDone)
            {
                onProgress?.Invoke(request.progress);
                yield return null;
            }

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                onError?.Invoke($"Download error: {uwr.error}");
                yield break;
            }
#else
            while (!uwr.isDone)
            {
                onProgress?.Invoke(uwr.downloadProgress);
                yield return null;
            }

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                onError?.Invoke($"Download error: {uwr.error}");
                yield break;
            }
#endif

            var bundle = DownloadHandlerAssetBundle.GetContent(uwr);
            if (bundle == null)
            {
                onError?.Invoke("Downloaded AssetBundle is null");
                yield break;
            }

            onLoaded?.Invoke(bundle);
        }
    }
    #endregion

    #region Load Asset From Bundle
    /// <summary>
    /// 이미 로드된 AssetBundle에서 특정 에셋을 비동기로 로드합니다.
    /// </summary>
    public Coroutine LoadAssetAsync<T>(AssetBundle bundle, string assetName, Action<T> onLoaded, Action<float> onProgress = null, Action<string> onError = null) where T : UnityEngine.Object
    {
        return StartCoroutine(LoadAssetCoroutine(bundle, assetName, onLoaded, onProgress, onError));
    }

    private IEnumerator LoadAssetCoroutine<T>(AssetBundle bundle, string assetName, Action<T> onLoaded, Action<float> onProgress, Action<string> onError) where T : UnityEngine.Object
    {
        if (bundle == null)
        {
            onError?.Invoke("AssetBundle is null");
            yield break;
        }

        if (string.IsNullOrEmpty(assetName))
        {
            onError?.Invoke("assetName is null or empty");
            yield break;
        }

        var request = bundle.LoadAssetAsync<T>(assetName);
        if (request == null)
        {
            onError?.Invoke($"Failed to start LoadAssetAsync: {assetName}");
            yield break;
        }

        while (!request.isDone)
        {
            onProgress?.Invoke(request.progress);
            yield return null;
        }

        var asset = request.asset as T;
        if (asset == null)
        {
            onError?.Invoke($"Loaded asset is null or wrong type: {assetName}");
            yield break;
        }

        onLoaded?.Invoke(asset);
    }
    #endregion

    #region Load Scene From Bundle
    /// <summary>
    /// AssetBundle 내부의 씬을 비동기로 로드합니다.
    /// 주의: 씬이 번들에 포함되어 있어야 하며, 씬 이름(또는 경로)을 정확히 알아야 합니다.
    /// </summary>
    public Coroutine LoadSceneFromBundleAsync(AssetBundle bundle, string scenePathOrName, LoadSceneMode mode = LoadSceneMode.Single, Action<float> onProgress = null, Action<string> onError = null, Action onDone = null)
    {
        return StartCoroutine(LoadSceneCoroutine(bundle, scenePathOrName, mode, onProgress, onError, onDone));
    }

    private IEnumerator LoadSceneCoroutine(AssetBundle bundle, string scenePathOrName, LoadSceneMode mode, Action<float> onProgress, Action<string> onError, Action onDone)
    {
        if (bundle == null)
        {
            onError?.Invoke("AssetBundle is null");
            yield break;
        }

        if (string.IsNullOrEmpty(scenePathOrName))
        {
            onError?.Invoke("scenePathOrName is null or empty");
            yield break;
        }

        // 씬을 번들에서 로드하려면 먼저 씬이름이 아닌 "씬 경로"(예: "Assets/Scenes/MyScene.unity")로 번들에 포함되어 있을 수 있으니
        // bundle.GetAllScenePaths()로 확인하는 것을 권장.
        var paths = bundle.GetAllScenePaths();
        string matched = null;
        foreach (var p in paths)
        {
            if (p.Contains(scenePathOrName))
            {
                matched = p;
                break;
            }
        }

        if (matched == null)
        {
            onError?.Invoke($"Scene not found in bundle: {scenePathOrName}");
            yield break;
        }

        var asyncLoad = SceneManager.LoadSceneAsync(matched, mode);
        if (asyncLoad == null)
        {
            onError?.Invoke($"Failed to start loading scene: {matched}");
            yield break;
        }

        while (!asyncLoad.isDone)
        {
            onProgress?.Invoke(asyncLoad.progress);
            yield return null;
        }

        onDone?.Invoke();
    }
    #endregion

    #region Unload Helpers
    /// <summary>
    /// 번들을 언로드합니다. (true -> 메모리에서 풀까지 언로드)
    /// </summary>
    public void UnloadBundle(AssetBundle bundle, bool unloadAllLoadedObjects = false)
    {
        if (bundle == null) return;
        bundle.Unload(unloadAllLoadedObjects);
    }
    #endregion

    #region Example Usages
    // 아래 샘플은 사용 예시입니다. 실제 프로젝트에서는 별도 관리자가 호출하도록 구성하세요.

    // 예: 로컬 파일에서 오브젝트 인스턴스화까지
    public void Example_LoadLocalAndInstantiate(string localPath, string prefabName)
    {
        LoadFromFileAsync(localPath, bundle =>
        {
            StartCoroutine(LoadAssetCoroutine<GameObject>(bundle, prefabName, go =>
            {
                Instantiate(go);
                // 필요시 번들 언로드 (인스턴스만 남기려면 true로 설정)
                UnloadBundle(bundle, false);
            }, p => Debug.Log($"asset progress: {p}"), err => Debug.LogError(err)));
        }, p => Debug.Log($"bundle progress: {p}"), err => Debug.LogError(err));
    }

    // 예: URL에서 씬 로드
    public void Example_LoadSceneFromUrl(string url, string sceneName)
    {
        LoadFromUrlAsync(url, bundle =>
        {
            StartCoroutine(LoadSceneCoroutine(bundle, sceneName, LoadSceneMode.Single, p => Debug.Log($"scene load: {p}"), err => Debug.LogError(err), () =>
            {
                Debug.Log("Scene loaded from bundle.");
                // 씬 로드 후 번들 언로드
                UnloadBundle(bundle, false);
            }));
        }, p => Debug.Log($"download progress: {p}"), err => Debug.LogError(err));
    }
    #endregion
}
