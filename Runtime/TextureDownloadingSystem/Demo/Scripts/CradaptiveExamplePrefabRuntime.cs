using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cradaptive.MultipleTextureDownloadSystem;

namespace Cradaptive.MultipleTextureDownloadSystem.Demo
{
    public class CradaptiveExamplePrefabRuntime : MonoBehaviour
    {
        public string url;
        public Image image;

        private void Awake()
        {
            Init();
        }

        public void Init()
        {
            TextureDownloadRequest textureDownloadRequest = new TextureDownloadRequest();
            textureDownloadRequest.url = url;
            textureDownloadRequest.OnTextureAvailable = (spr,response) =>
            {
                image.sprite = spr;
            };
            CradaptiveTexturesDownloader.QueueForDownload(textureDownloadRequest);
        }
    }
}