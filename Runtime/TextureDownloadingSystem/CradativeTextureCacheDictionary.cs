using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Cradaptive.MultipleTextureDownloadSystem
{

    [System.Serializable]
    public class CradaptiveTextureCache
    {
        public Sprite sprite;
        public string url;
    }

    [System.Serializable]
    public class CradativeTextureCacheDictionary
    {
        public List<CradaptiveTextureCache> textureSaveClasses = new List<CradaptiveTextureCache>();
        public bool Contains(string url)
        {
            CradaptiveTextureCache textureSaveClass = textureSaveClasses.FirstOrDefault(x => x.url == url);
            if (textureSaveClass != null)
                return true;
            return false;
        }
        public bool Contains(ICradaptiveTextureOwner url)
        {
            CradaptiveTextureCache textureSaveClass = textureSaveClasses.FirstOrDefault(x => x.url == url.url);
            if (textureSaveClass != null)
                return true;
            return false;
        }

        public void Add(CradaptiveTextureCache textureSaveClass)
        {
            if (!Contains(textureSaveClass.url))
                textureSaveClasses.Add(textureSaveClass);
        }
        public void Remove(CradaptiveTextureCache textureSaveClass)
        {
            if (Contains(textureSaveClass.url))
                textureSaveClasses.Remove(textureSaveClass);
        }
        public void Remove(ICradaptiveTextureOwner url)
        {
            CradaptiveTextureCache[] textureSaveClass = textureSaveClasses.Where(x => x.url == url.url).ToArray();

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
            CradaptiveTextureCache textureSaveClass = textureSaveClasses.FirstOrDefault(x => x.url == url);
            if (textureSaveClass != null)
                Remove(textureSaveClass);
        }
        public CradaptiveTextureCache GetTextureSaveClass(string url)
        {
            CradaptiveTextureCache textureSaveClass = textureSaveClasses.FirstOrDefault(x => x.url == url);
            if (textureSaveClass != null)
                return textureSaveClass;
            return null;
        }
        public CradaptiveTextureCache GetTextureSaveClass(ICradaptiveTextureOwner url)
        {
            CradaptiveTextureCache textureSaveClass = textureSaveClasses.FirstOrDefault(x => x.url == url.url);
            if (textureSaveClass != null)
                return textureSaveClass;
            return null;
        }
        public CradaptiveTextureCache First()
        {
            CradaptiveTextureCache textureSaveClass = textureSaveClasses.First();
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