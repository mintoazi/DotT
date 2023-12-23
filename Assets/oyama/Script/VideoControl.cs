using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

public class VideoControl : MonoBehaviour
{
    [SerializeField] VideoPlayer videoPlayer;
    private object videoClipList;
    private bool loop;
    private int videoIndex;

    //void Start()
    //{
    //    StartCoroutine(playVideo());  // コルーチン実行
    //}

    //IEnumerator playVideo(bool firstRun = true)
    //{  // コルーチン宣言
    //    int listLen = videoClipList.Count;
    //    if (videoClipList == null || listLen <= 0)
    //    {
    //        Debug.LogError("Assign VideoClips from the Editor");
    //        yield break;
    //    }

    //    if (loop)  // ループの処理
    //        videoIndex %= listLen;
    //    else
    //    {  // 全再生完了で終了
    //        if (videoIndex >= listLen) yield break;
    //    }

    //}

    void Update()
    {
        
    }
}