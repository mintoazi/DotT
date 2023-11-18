using UnityEngine;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;

public class CardMovement : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField] private Card myCard;
    [SerializeField] private bool isEnemy = false;
    public bool IsEnemy{ set { isEnemy = value; } }
    
    public Transform defaultParent;
    private bool isHand = true;

    private CanvasGroup canvasGroup;
    private Vector3 selectCardSize = new Vector3(2f, 2f, 2f);
    private Vector3 handPosition;
    private float moveTime = 0.2f;
    private bool isMove = false;

    public bool IsHand { get { return isHand; } set { isHand = value;  } }

    public Card MyCard { get { return myCard; } }
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isMove || isEnemy) return;
        if (IsHand) handPosition = transform.position;
        //Debug.Log(transform.position);
        defaultParent = transform.parent;// .parent
        transform.SetParent(defaultParent, false);
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isEnemy) return;
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isEnemy) return;
        if (IsHand) MoveToSelect(trans: defaultParent, isSelect: false).Forget();
        //transform.SetParent(defaultParent, true);
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
       // canvasGroup.blocksRaycasts = true;
    }

    public async UniTask MoveToSelect(Transform trans, bool isSelect)
    {
        if (isEnemy) return;
        float time = 0f;
        Vector3 startPos = transform.position;
        Vector3 startSize = transform.localScale;

        isMove = true;
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
        isMove = false;
    }
}
