using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEditor.ShaderGraph.Internal;

public class CardMovement : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public Transform defaultParent;
    private bool isHand = true;

    private CanvasGroup canvasGroup;
    private Vector3 selectCardSize = new Vector3(2f, 2f, 2f);
    private Vector3 handPosition;
    private float moveTime = 0.2f;
    private bool isMove = false;

    public bool IsHand { get { return isHand; } set { isHand = value; Debug.Log(isHand); } }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isMove) return;
        if (IsHand) handPosition = transform.position;
        Debug.Log(transform.position);
        defaultParent = transform;// .parent
        transform.SetParent(defaultParent.parent, false);
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(defaultParent, true);
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = true;
    }

    public async UniTask MoveToSelect(Transform trans, bool isSelect)
    {
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
                transform.localScale = Vector3.Lerp(startSize, Vector3.one, time / moveTime);
                transform.position = Vector3.Lerp(startPos, handPosition, time / moveTime);
            }
            await UniTask.DelayFrame(1);
        }
        isMove = false;
    }
}
