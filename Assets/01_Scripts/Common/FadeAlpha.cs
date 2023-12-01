using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public static class FadeAlpha
{
    public static async UniTask FadeIn(Image target)
    {
        float start = 1f;
        float end = 0f;
        float step = 0f;
        Color c = target.color;

        c.a = start;
        target.color = c;

        while(step >= end)
        {
            step += Time.deltaTime;
            c.a = Mathf.Lerp(start, end, step);
            target.color = c;
            await UniTask.DelayFrame(1);
        }
    }

    public static async UniTask FadeOut(Image target)
    {
        float start = 0f;
        float end = 1f;
        float step = 0f;
        Color c = target.color;

        c.a = start;
        target.color = c;

        while (step >= end)
        {
            step += Time.deltaTime;
            c.a = Mathf.Lerp(start, end, step);
            target.color = c;
            await UniTask.DelayFrame(1);
        }
    }
}
