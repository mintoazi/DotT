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
    //    StartCoroutine(playVideo());  // �R���[�`�����s
    //}

    //IEnumerator playVideo(bool firstRun = true)
    //{  // �R���[�`���錾
    //    int listLen = videoClipList.Count;
    //    if (videoClipList == null || listLen <= 0)
    //    {
    //        Debug.LogError("Assign VideoClips from the Editor");
    //        yield break;
    //    }

    //    if (loop)  // ���[�v�̏���
    //        videoIndex %= listLen;
    //    else
    //    {  // �S�Đ������ŏI��
    //        if (videoIndex >= listLen) yield break;
    //    }

    //}

    void Update()
    {
        
    }
}