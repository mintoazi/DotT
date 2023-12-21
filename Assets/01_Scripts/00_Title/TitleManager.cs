using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleManager : MonoBehaviour
{
    [SerializeField] private Image fadePanel;

    private void Awake()
    {
        StartFade().Forget();
    }
    private async UniTask StartFade()
    {
        await FadeAlpha.FadeIn(fadePanel);
        await UniTask.WaitUntil(() => Input.GetMouseButtonDown(0));
        await FadeAlpha.FadeOut(fadePanel);
        LoadHomeScene();
    }

    public void LoadHomeScene()
    {
        SceneManager.LoadScene("01_Home");
    }
}
