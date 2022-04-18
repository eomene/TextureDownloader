using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cradaptive.MultipleTextureDownloadSystem;

namespace Cradaptive.MultipleTextureDownloadSystem.Demo
{
    public class CradaptiveExamplePrefabServerData : MonoBehaviour
    {
        public Image image;

        public void Init(CradaptiveSimpleData cradaptiveSimpleData)
        {
            cradaptiveSimpleData.OnTextureAvailable = (spr, response) =>
            {
                if (image)
                    image.sprite = spr;
            };
            CradaptiveTexturesDownloader.QueueForDownload(cradaptiveSimpleData);
        }
    }
}