using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cradaptive.MultipleTextureDownloadSystem;
using UnityEngine.UI;
using System;

namespace Cradaptive.MultipleTextureDownloadSystem.Demo
{
    [System.Serializable]
    public class CradaptiveSimpleData : ICradaptiveTextureOwner
    {
        public string name;
        /// <summary>
        /// Url for image or local name in the Downloaded textures dictionary (if you have the image locally assigned)
        /// </summary>
        public string url { get => serialisedUrl; set => serialisedUrl = value; }

        public string serialisedUrl;
        /// <summary>
        /// Callback for when image is available
        /// </summary>
        public Action<Sprite> OnTextureAvailable { get; set; }

    }

    public class CradaptiveTextureSaverDemo : MonoBehaviour
    {
        [SerializeField]
        Transform parent;
        [SerializeField]
        GameObject prefab;
        [SerializeField]
        List<CradaptiveSimpleData> listData = new List<CradaptiveSimpleData>();

        private void Awake()
        {
            Populate();
        }

        public void ClearParent()
        {
            foreach (Transform tr in parent)
                Destroy(tr.gameObject);
        }

        public void Populate()
        {
            ClearParent();
            foreach (var data in listData)
            {
                CradaptiveExamplePrefabServerData lisPrefab = Instantiate(prefab, parent)?.GetComponent<CradaptiveExamplePrefabServerData>();
                lisPrefab?.Init(data);
            }
        }
    }
}


