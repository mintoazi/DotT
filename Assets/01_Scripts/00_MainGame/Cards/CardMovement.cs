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
        //Debug.Log(eventData.pointerCurrentRaycast.gameObject.name);

        // シャドバ風に処理
        // blockRaycasts 視覚化処理
        // pointerCurrentRaycast.gameObject.CompareTag("Field"){
        // 
        // }
        Vector3 pos = transform.position;
        pos.x = eventData.position.x;
        pos.y = eventData.position.y;
        transform.position = pos;
        //transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (IsHand) MoveToSelect(trans: defaultParent, isSelect: false).Forget();
    }

    public async UniTask MoveToSelect(Transform trans, bool isSelect)
    {
        if (isSelect)
        {
            myCard.ResizeCard(selectCardSize, moveTime).Forget();
            await myCard.MoveCard(trans.position, moveTime);
        }
        else
        {
            if (transform == null) return;
            myCard.ResizeCard(Vector3.one, moveTime).Forget();
            await myCard.MoveCard(handPosition, moveTime);
            canvasGroup.blocksRaycasts = true;
        }
    }
}
