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
        FadeAlpha.FadeIn(fadePanel).Forget();
    }

    public void LoadHomeScene()
    {
        SceneManager.LoadScene("01_Home");
    }
}
