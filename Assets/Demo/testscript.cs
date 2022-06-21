using System.Collections;
using System.Collections.Generic;
using Cradaptive.MultipleTextureDownloadSystem;
using UnityEngine;

public class testscript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
       
        for (int i = 0; i < 10000; i++)
        {
            int index = i;
            TextureDownloadRequest requester = new TextureDownloadRequest()
            {
                url = "https://picsum.photos/200/300?random=1",
                OnTextureAvailable = (result, msg) =>
                {
                    Debug.LogError($" downloaded {index}");
                }
            };
            
            CradaptiveTexturesDownloader.QueueForDownload(requester);
         //   index++;
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
