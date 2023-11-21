using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;

public class CardMovement : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] private Card myCard;
    [SerializeField] private Image grayoutPanel;
    [SerializeField] private bool isEnemy = false;
    public bool IsEnemy{ set { isEnemy = value; } }
    
    public Transform defaultParent;
    private bool isHand = true;

    private CanvasGroup canvasGroup;
    private Vector3 selectCardSize = new Vector3(2f, 2f, 2f);
    private Vector3 handPosition;
    private float moveTime = 0.2f;

    public bool IsHand { get { return isHand; } set { isHand = value;  } }

    public Card MyCard { get { return myCard; } }

    public void SetSelectable(bool value)
    {
        canvasGroup.blocksRaycasts = value;
        grayoutPanel.enabled = !value;

    }
    private void OnEnable()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (isEnemy) canvasGroup.blocksRaycasts = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // カード情報の拡大表示    

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (IsHand) handPosition = transform.position; // 元に戻すときの場所を保存
        defaultParent = transform.parent;
        transform.SetParent(defaultParent, false);
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (IsHand) MoveToSelect(trans: defaultParent, isSelect: false).Forget();
    }

    public async UniTask MoveToSelect(Transform trans, bool isSelect)
    {
        float time = 0f;
        Vector3 startPos = transform.position;
        Vector3 startSize = transform.localScale;

        while(time < moveTime)
        {
            time += Time.deltaTime;
            if (time > moveTime) time = moveTime;
            if (isSelect)
            {
                transform.localScale = Vector3.Lerp(startSize, selectCardSize, time / moveTime);
                transform.position = Vector3.Lerp(startPos, trans.position, time / moveTime);
            }
            else
            {
                if (transform == null) return;
                transform.localScale = Vector3.Lerp(startSize, Vector3.one, time / moveTime);
                transform.position = Vector3.Lerp(startPos, handPosition, time / moveTime);
                canvasGroup.blocksRaycasts = true;
            }
            await UniTask.DelayFrame(1);
        }
    }
}
