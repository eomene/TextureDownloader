#define USE_ADDRESSABLES


using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
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

        public void Awake()
        {
            Instance = this;
            Initialise();
        }

        private static void Initialise()
        {
            if (Instance == null)
            {
                GameObject downloader = new GameObject("CradaptiveTextureDownloader");
                Instance = downloader.AddComponent<CradaptiveTexturesDownloader>();
            }

            Sprite[] loadedSprites = Resources.LoadAll<Sprite>("CradaptiveTextures");
            for (int i = 0; i < loadedSprites.Length; i++)
            {
                if (!Instance.downloadedTextures.Contains(loadedSprites[i].name))
                    Instance.downloadedTextures.Add(new CradaptiveTextureCache
                        {sprite = loadedSprites[i], url = loadedSprites[i].name});
            }
        }

        public static void QueueForDownload(ICradaptiveTextureOwner previewOwner)
        {
            Initialise();

            if (previewOwner == null || string.IsNullOrEmpty(previewOwner.url)) return;

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
                Instance.StartCoroutine(DownloadPreviewImagesAsync());
            }
            else
            {
                screenShotDownloadQueue.AddCallbackAction(previewOwner);
            }
        }

        private static IEnumerator DownloadPreviewImagesAsync()
        {
            if (screenShotDownloadQueue.Count() > 0)
            {
                CradaptiveTextureRequestsCache processingObject;

                lock (screenShotDownloadQueue)
                {
                    processingObject = screenShotDownloadQueue.First();
                    screenShotDownloadQueue.Remove(processingObject);
                    processingObject.downloadAttempts++;
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
                Debug.LogError("Error: " + processingObject.url + "/" + uwr.error + uwr.downloadHandler?.text);
                if (processingObject.downloadAttempts <= 3)
                    screenShotDownloadQueue.Add(processingObject);
                SendCallbackResult(processingObject, null, "Failed trials to download from server");
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                SendCallbackResult(processingObject, texture, "");
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
            }
            catch (Exception e)
            {
                SendCallbackResult(processingObject, null, e.Message);
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
            }
            else
            {
                SendCallbackResult(processingObject, null, opHandle.OperationException.Message);
            }
#else
            SendCallbackResult(processingObject, null, "Addressables not enabled, please install addresables package and add define symbols");
            Debug.LogError("Addressables not enabled, please install addresables package and add define symbols");
#endif
        }

        private static Sprite CreateSprite(Texture2D texture)
        {
            if (texture == null)
            {
                Debug.LogError($"No texture for asset {texture.name}");
                return null;
            }
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f));
            return sprite;
        }

        private static void SendCallbackResult(CradaptiveTextureRequestsCache processingObject, Texture2D texture,
            string message)
        {
            var sprite = CreateSprite(texture);
            for (int i = 0; i < processingObject.actions.Count; i++)
            {
                processingObject.actions[i]?.Invoke(sprite, message);
            }

            if (sprite == null) return;
            if (!Instance.downloadedTextures.Contains(processingObject.url))
                Instance.downloadedTextures.Add(new CradaptiveTextureCache
                    {url = processingObject.url, sprite = sprite});
        }
    }
}