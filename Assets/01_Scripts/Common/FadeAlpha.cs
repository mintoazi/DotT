using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using UnityEngine.UI;

public static class FadeAlpha
{
    public static async UniTask FadeIn(Image target)
    {
        float start = 1f;
        float end = 0f;
        float time = 0f;
        float fadeTime = 2f;
        Color c = target.color;

        c.a = start;
        target.color = c;

        while(time <= fadeTime)
        {
            time += Time.deltaTime;
            float step = time / fadeTime;
            c.a = Mathf.Lerp(start, end, step);
            target.color = c;
            await UniTask.Yield();
        }
    }

    public static async UniTask FadeOut(Image target)
    {
        float start = 0f;
        float end = 1f;
        float time = 0f;
        float fadeTime = 2f;
        Color c = target.color;

        c.a = start;
        target.color = c;

        while (time <= fadeTime)
        {
            time += Time.deltaTime;
            float step = time / fadeTime;
            c.a = Mathf.Lerp(start, end, step);
            target.color = c;
            await UniTask.Yield();
        }
    }
}
