using UnityEngine;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.Build;

[CreateAssetMenu(fileName = "CradaptiveTextureConfig", menuName = "CradaptiveTextureDownloader/CradaptiveTextureConfig")]
public class CradaptiveTextureConfig : ScriptableObject
{
    public bool initOnAwake = true;
    public string pathToResourcesFolder = "CradaptiveTextures";
    public string tagToAddressables = "CradaptiveTextures";
    public bool dontDestroyOnLoad = true;
    public bool useResourcesFolderAssets = true;
    public bool enableAddressables = false;
    public int maximumNoOfDownloads = 7;

    [ContextMenu("Enable Addressables")]
    public void EnableAddressables()
    {
        PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Android, "USE_ADDRESSABLES");
        PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.WebGL, "USE_ADDRESSABLES");
        PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.WindowsStoreApps, "USE_ADDRESSABLES");
        PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.iOS, "USE_ADDRESSABLES");
        PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.XboxOne, "USE_ADDRESSABLES");
        PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.tvOS, "USE_ADDRESSABLES");
        PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Server, "USE_ADDRESSABLES");
        PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Stadia, "USE_ADDRESSABLES");
        PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Standalone, "USE_ADDRESSABLES");
    }
    
    [ContextMenu("Remove Addressables Support")]
    public void RemoveAddressables()
    {
       // var PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Android);
       //  PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.WebGL);
       //  PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.WindowsStoreApps);
       //  PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.iOS);
       //  PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.XboxOne);
       //  PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.tvOS);
       //  PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Server);
       //  PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Stadia);
       //  PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone);
    }
}

