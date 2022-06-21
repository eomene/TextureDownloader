//#define USE_ADDRESSABLES


using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
#if USE_ADDRESSABLES
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
#endif
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;


namespace Cradaptive.MultipleTextureDownloadSystem
{
    public class CradaptiveTexturesDownloader : MonoBehaviour
    {
        private static CradaptiveTexturesDownloader Instance;

        private static CradativeTextureRequestsCacheDictionary screenShotDownloadQueue =
            new CradativeTextureRequestsCacheDictionary();

        public CradativeTextureCacheDictionary downloadedTextures = new CradativeTextureCacheDictionary();
        public Texture2D defaultTexture;
        private static bool isInitialised;
        public List<string> badListOfFailedTextxures = new List<string>();
        private static CradaptiveTextureConfig cradaptiveTextureConfig;
        private int currentDownloadingCount;

        public void Awake()
        {
            cradaptiveTextureConfig =
                Resources.Load<CradaptiveTextureConfig>("CradaptiveTexturesSettings/CradaptiveTextureConfig");
            if (cradaptiveTextureConfig.initOnAwake)
                Initialise();
        }

        public static void Initialise()
        {
            if (isInitialised)
                return;
            isInitialised = true;

            if (Instance == null)
            {
                GameObject downloader = new GameObject("CradaptiveTextureDownloader");
                Instance = downloader.AddComponent<CradaptiveTexturesDownloader>();
                if (cradaptiveTextureConfig.dontDestroyOnLoad)
                    DontDestroyOnLoad(Instance.gameObject);
            }


            if (cradaptiveTextureConfig.useResourcesFolderAssets)
            {
                Sprite[] loadedSprites = Resources.LoadAll<Sprite>(cradaptiveTextureConfig.pathToResourcesFolder);
                for (int i = 0; i < loadedSprites.Length; i++)
                {
                    if (!Instance.downloadedTextures.Contains(loadedSprites[i].name))
                        Instance.downloadedTextures.Add(new CradaptiveTextureCache
                            {sprite = loadedSprites[i], url = loadedSprites[i].name});
                }
            }
        }

        public static void QueueForDownload(ICradaptiveTextureOwner previewOwner)
        {
            Initialise();

            if (previewOwner == null || string.IsNullOrEmpty(previewOwner.url) ||
                Instance.badListOfFailedTextxures.Contains(previewOwner.url)) return;

            if (Instance.downloadedTextures.Contains(previewOwner.url))
            {
                previewOwner.OnTextureAvailable(Instance.downloadedTextures.GetTextureSaveClass(previewOwner.url)
                    ?.sprite, "");
                return;
            }

            ICradaptiveDownloadLocation localTexture = previewOwner as ICradaptiveDownloadLocation;

            if (localTexture != null && (localTexture.currentDownloadType == DownloadType.web &&
                                         !previewOwner.url.Contains("http")))
                return;

            if (!screenShotDownloadQueue.Contains(previewOwner.url))
            {
                screenShotDownloadQueue.Add(new CradaptiveTextureRequestsCache
                {
                    url = previewOwner.url,
                    actions = new List<Action<Sprite, string>> {previewOwner.OnTextureAvailable},
                    currentDownloadType = localTexture?.currentDownloadType ?? DownloadType.web
                });


                Instance.StartFetching();
            }
            else
            {
                screenShotDownloadQueue.AddCallbackAction(previewOwner);
            }
        }

        public void StartFetching()
        {
            Instance.StartCoroutine(DownloadPreviewImagesAsync());
        }

        private static IEnumerator DownloadPreviewImagesAsync()
        {
            if (screenShotDownloadQueue.Count() > 0 &&
                cradaptiveTextureConfig.maximumNoOfDownloads > Instance.currentDownloadingCount)
            {
                CradaptiveTextureRequestsCache processingObject;

                lock (screenShotDownloadQueue)
                {
                    processingObject = screenShotDownloadQueue.First();
                    screenShotDownloadQueue.Remove(processingObject);
                    processingObject.downloadAttempts++;
                    Instance.currentDownloadingCount++;
                }

                switch (processingObject.currentDownloadType)
                {
                    case DownloadType.web:
                        yield return Instance.GrabTextureFromServer(processingObject);
                        break;
                    case DownloadType.local:
                        yield return Instance.GrabTextureLocally(processingObject);
                        break;
                    case DownloadType.assetBundle:
                        yield return Instance.GrabFromAssetBundle(processingObject);
                        break;
                }
            }
            else
            {
                yield return new WaitForSeconds(.5f);
            }

            yield return Instance.StartCoroutine(DownloadPreviewImagesAsync());
        }


        private IEnumerator GrabTextureFromServer(CradaptiveTextureRequestsCache processingObject)
        {
            UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(processingObject.url);
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.ConnectionError ||
                uwr.result == UnityWebRequest.Result.ProtocolError)
            {
#if UNITY_EDITOR
                Debug.LogError("Error: " + processingObject.url + "/" + uwr.error + uwr.downloadHandler?.text);
#endif
                if (processingObject.downloadAttempts <= 3)
                {
                    screenShotDownloadQueue.Add(processingObject);
                }
                else
                {
                    Instance.badListOfFailedTextxures.Add(processingObject.url);
                    SendCallbackResult(processingObject, null, "Failed trials to download from server");
                    Instance.currentDownloadingCount--;
                }
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                SendCallbackResult(processingObject, texture, "");
                Instance.currentDownloadingCount--;
            }
        }

        private IEnumerator GrabTextureLocally(CradaptiveTextureRequestsCache processingObject)
        {
            yield return new WaitForEndOfFrame();
            try
            {
                byte[] byteArray = File.ReadAllBytes(processingObject.url);
                Texture2D loadedTexture = new Texture2D(8, 8);
                loadedTexture.LoadImage(byteArray);
                SendCallbackResult(processingObject, loadedTexture, "");
                Instance.currentDownloadingCount--;
            }
            catch (Exception e)
            {
                SendCallbackResult(processingObject, null, e.Message);
                Instance.currentDownloadingCount--;
            }
        }

        private IEnumerator GrabFromAssetBundle(CradaptiveTextureRequestsCache processingObject)
        {
            yield return new WaitForEndOfFrame();
#if USE_ADDRESSABLES
            AsyncOperationHandle<Texture2D> opHandle = Addressables.LoadAssetAsync<Texture2D>(processingObject.url);
            yield return opHandle;

            if (opHandle.Status == AsyncOperationStatus.Succeeded)
            {
                Texture2D loadedTexture = opHandle.Result;
                SendCallbackResult(processingObject, loadedTexture, "");
  Instance.currentDownloadingCount--;
            }
            else
            {
                SendCallbackResult(processingObject, null, opHandle.OperationException.Message);
  Instance.currentDownloadingCount--;
            }
#else
            SendCallbackResult(processingObject, null,
                "Addressables not enabled, please install addresables package and add define symbols");
            Debug.LogError("Addressables not enabled, please install addresables package and add define symbols");
#endif
        }

        public static Sprite CreateSprite(Texture2D texture, string name)
        {
            if (texture == null)
            {
                // Debug.LogError($"No texture for asset {name}");
                return null;
            }

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));
            return sprite;
        }

        private static void SendCallbackResult(CradaptiveTextureRequestsCache processingObject, Texture2D texture,
            string message)
        {
            string errorMessage = $"No texture for asset {processingObject.url}";
            if (texture == null)
            {
                Instance.UpdateProcessingObjects(processingObject, null, errorMessage);
#if UNITY_EDITOR
                Debug.LogError(errorMessage);
#endif
                return;
            }

            var sprite = CreateSprite(texture, processingObject.url);
            Instance.UpdateProcessingObjects(processingObject, sprite, message);
            if (sprite == null)
            {
                Instance.UpdateProcessingObjects(processingObject, null, errorMessage);
                //   Debug.LogError(errorMessage);
                return;
            }

            if (!Instance.downloadedTextures.Contains(processingObject.url))
                Instance.downloadedTextures.Add(new CradaptiveTextureCache
                    {url = processingObject.url, sprite = sprite});
        }

        void UpdateProcessingObjects(CradaptiveTextureRequestsCache processingObject, Sprite sprite, string message)
        {
            for (int i = 0; i < processingObject.actions.Count; i++)
            {
                processingObject.actions[i]?.Invoke(sprite, message);
            }
        }
    }
}