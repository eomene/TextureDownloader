using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;

[CreateAssetMenu(fileName = "CradaptiveTextureConfig", menuName = "CradaptiveTextureDownloader/CradaptiveTextureConfig")]
public class CradaptiveTextureConfig : ScriptableObject
{
    public bool initOnAwake = true;
    public string pathToResourcesFolder = "CradaptiveTextures";
    public string tagToAddressables = "CradaptiveTextures";
    public bool dontDestroyOnLoad = true;
    public bool useResourcesFolderAssets = true;
}

