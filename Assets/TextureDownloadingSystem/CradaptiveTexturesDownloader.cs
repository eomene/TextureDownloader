using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


namespace Cradaptive.MultipleTextureDownloadSystem
{
    public class CradaptiveTexturesDownloader : MonoBehaviour
    {
        static CradaptiveTexturesDownloader Instance;
        public static CradativeTextureRequestsCacheDictionary screenShotDownloadQueue = new CradativeTextureRequestsCacheDictionary();
        public CradativeTextureCacheDictionary downloadedTextures = new CradativeTextureCacheDictionary();
        public Texture2D defaultTexture;
        /// <summary>
        /// Here we set how many couroutines we want to be able to use for our downloads, you can increase based on the type of game
        /// </summary>
        private static Coroutine[] downloaders = new Coroutine[20];

        public void Awake()
        {
            Instance = this;
            Initialise();
        }

        static void Initialise()
        {
            if(Instance == null)
            {
                GameObject downloader = new GameObject("CradaptiveTextureDownloader");
                Instance = downloader.AddComponent<CradaptiveTexturesDownloader>();
            }
            Sprite[] loadedSprites = Resources.LoadAll<Sprite>("CradaptiveTextures");
            for (int i = 0; i < loadedSprites.Length; i++)
            {
                if (!Instance.downloadedTextures.Contains(loadedSprites[i].name))
                    Instance.downloadedTextures.Add(new CradaptiveTextureCache { sprite = loadedSprites[i], url = loadedSprites[i].name });
            }
        }

        public static void QueueForDownload(ICradaptiveTextureOwner previewOwner)
        {
            Initialise();

            if (previewOwner == null || string.IsNullOrEmpty(previewOwner.url)) return;

            if (Instance.downloadedTextures.Contains(previewOwner.url))
            {
                previewOwner.OnTextureAvailable(Instance.downloadedTextures.GetTextureSaveClass(previewOwner.url)?.sprite);
                return;
            }

            if (!previewOwner.url.Contains("http"))
                return;

            if (!screenShotDownloadQueue.Contains(previewOwner.url))
            {
                screenShotDownloadQueue.Add(new CradaptiveTextureRequestsCache { url = previewOwner.url, actions = new List<Action<Sprite>> { previewOwner.OnTextureAvailable } });
                Coroutine downloader = downloaders.FirstOrDefault(x => x == null);
                if (downloader == null)
                    downloader = Instance.StartCoroutine(DownloadPreviewImagesAsync());
                else
                {
                    Debug.LogError("Hey, we are using all courotines available at the moment");
                }
                
            }
            else
            {
                screenShotDownloadQueue.AddCallbackAction(previewOwner);
            }
        }

        /// <summary>
        /// used to download image previews
        /// </summary>
        /// <returns></returns>
        private static IEnumerator DownloadPreviewImagesAsync()
        {
            if (screenShotDownloadQueue.Count() > 0)
            {
                CradaptiveTextureRequestsCache processingObject;
                int downloadAttempts = 0;
                lock (screenShotDownloadQueue)
                {
                    processingObject = screenShotDownloadQueue.First();
                    screenShotDownloadQueue.Remove(processingObject);
                    downloadAttempts++;
                }
                string newUrl = processingObject.url;
                UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(newUrl);
                yield return uwr.SendWebRequest();
                if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError("Error: " + processingObject.url + "/" + uwr.error + uwr.downloadHandler?.text);
                    if (downloadAttempts <= 3)
                        screenShotDownloadQueue.Add(processingObject);
                    //TODO: Tell the class expecting a callback none is coming because we couldnt find the image anywhere
                }
                else
                {
                    Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    for (int i = 0; i < processingObject.actions.Count; i++)
                    {
                        processingObject.actions[i]?.Invoke(sprite);
                    }

                    if (!Instance.downloadedTextures.Contains(processingObject.url))
                        Instance.downloadedTextures.Add(new CradaptiveTextureCache { url = processingObject.url, sprite = sprite });
                }
            }
            else
            {
                yield return new WaitForSeconds(.5f);
            }
            yield return Instance.StartCoroutine(DownloadPreviewImagesAsync());
        }
    }

}