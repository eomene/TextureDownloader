using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cradaptive.MultipleTextureDownloadSystem
{
    public enum DownloadType
    {
        local,
        web,
        assetBundle
    };
    [System.Serializable]
    public class CradaptiveTextureRequestsCache
    {
        public List<Action<Sprite,string>> actions;
        public string url;
        public int downloadAttempts;
        public DownloadType currentDownloadType;
    }

    [System.Serializable]
    public class CradativeTextureRequestsCacheDictionary
    {
        public List<CradaptiveTextureRequestsCache> textureSaveClasses = new List<CradaptiveTextureRequestsCache>();
        public bool Contains(string url)
        {
            CradaptiveTextureRequestsCache textureSaveClass = textureSaveClasses.FirstOrDefault(x => x.url == url);
            if (textureSaveClass != null)
                return true;
            return false;
        }

        public void AddCallbackAction(ICradaptiveTextureOwner owner)
        {
            textureSaveClasses.FirstOrDefault(x => x.url == owner.url)?.actions.Add(owner.OnTextureAvailable);
        }

        public void Add(CradaptiveTextureRequestsCache textureSaveClass)
        {
            if (!Contains(textureSaveClass.url))
                textureSaveClasses.Add(textureSaveClass);
        }
        public void Remove(CradaptiveTextureRequestsCache textureSaveClass)
        {
            if (Contains(textureSaveClass.url))
                textureSaveClasses.Remove(textureSaveClass);
        }
        public void Remove(ICradaptiveTextureOwner url)
        {
            CradaptiveTextureRequestsCache[] textureSaveClass = textureSaveClasses.Where(x => x.url == url.url).ToArray();

            if (textureSaveClass != null)
            {
                for (int i = 0; i < textureSaveClass.Length; i++)
                {
                    if (textureSaveClasses.Contains(textureSaveClasses[i]))
                        textureSaveClasses.Remove(textureSaveClass[i]);
                }
            }
        }
        public void Remove(string url)
        {
            CradaptiveTextureRequestsCache textureSaveClass = textureSaveClasses.FirstOrDefault(x => x.url == url);
            if (textureSaveClass != null)
                Remove(textureSaveClass);
        }
        public CradaptiveTextureRequestsCache GetTextureSaveClass(string url)
        {
            CradaptiveTextureRequestsCache textureSaveClass = textureSaveClasses.FirstOrDefault(x => x.url == url);
            if (textureSaveClass != null)
                return textureSaveClass;
            return null;
        }
        public CradaptiveTextureRequestsCache GetTextureSaveClass(ICradaptiveTextureOwner url)
        {
            CradaptiveTextureRequestsCache textureSaveClass = textureSaveClasses.FirstOrDefault(x => x.url == url.url);
            if (textureSaveClass != null)
                return textureSaveClass;
            return null;
        }
        public CradaptiveTextureRequestsCache First()
        {
            CradaptiveTextureRequestsCache textureSaveClass = textureSaveClasses.First();
            if (textureSaveClass != null)
                return textureSaveClass;
            return null;
        }

        public int Count()
        {
            return textureSaveClasses.Count();
        }
    }

}