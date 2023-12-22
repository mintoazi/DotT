using Cysharp.Threading.Tasks;
using UnityEngine;
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
        LoadHomeScene();
    }

    public void LoadHomeScene()
    {
        SceneLoader.Instance.Load(Scenes.Scene.HOME).Forget();
    }
}
