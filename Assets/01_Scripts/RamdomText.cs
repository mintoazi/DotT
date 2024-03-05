using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RamdomText : MonoBehaviour
{
    [SerializeField] Text title;
    [SerializeField] Text subTitle;
    [SerializeField] Text info;

    [SerializeField] string[] titles;
    [SerializeField]
    string[] subTitles;
    [SerializeField] string[] infos;

    public void StartLoad()
    {
        int random = Random.Range(0, titles.Length);
        title.text = titles[random];
        subTitle.text = subTitles[random];
        info.text = infos[random];
        AlphaSet();
    }

    private void AlphaSet()
    {
        FadeAlpha.FadeOut(title).Forget();
        FadeAlpha.FadeOut(subTitle).Forget();
        FadeAlpha.FadeOut(info).Forget();
    }
    public void FadeIn()
    {
        FadeAlpha.FadeIn(title).Forget();
        FadeAlpha.FadeIn(subTitle).Forget();
        FadeAlpha.FadeIn(info).Forget();
    }
}
