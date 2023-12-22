using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Playables;

public class OnCursor : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [SerializeField]
    private PlayableDirector director;
    [SerializeField]
    private UIOutline uiOutline;
    [SerializeField]
    private float flashOutlineWidth = 300f;
    [SerializeField]
    private float flashTime = 0.3f;
    [SerializeField]
    private RectTransform buttonBase;
    [SerializeField]
    private float clickedSize;
    [SerializeField]
    private Home.Buttons myButton;

    bool onClicked = false;

    private void Update() { } // Inspector表示用
    public void OnPointerClick(PointerEventData eventData)
    {
        // 1回目のみ処理
        if (onClicked) return;
        onClicked = true;

        // クリックアニメーション
        OnClick().Forget();
    }
    private async UniTask OnClick()
    {
        float currentTime = 0f;
        float defSize = buttonBase.localScale.x;
        bool flash = true;

        uiOutline.ResizeOutline(flashOutlineWidth, flashTime).Forget();
        // ボタンの伸縮、アウトラインのフラッシュ
        while (true)
        {
            if(flash) currentTime += Time.deltaTime;
            else currentTime -= Time.deltaTime;

            float step = currentTime / flashTime;
            buttonBase.localScale = Vector3.one * Mathf.Lerp(defSize, clickedSize, step);

            if (currentTime > flashTime) flash = false;
            else if (currentTime < 0) break;
            await UniTask.Yield();
        }

        switch (myButton)
        {
            case Home.Buttons.B_ROOM:
                SceneLoader.Instance.Load(Scenes.Scene.MATCHING).Forget();
                break;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (onClicked) return;
        director.Play();
    }

}
