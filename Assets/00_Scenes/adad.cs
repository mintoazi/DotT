using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class adad : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        GetComponent<CanvasGroup>().blocksRaycasts = true;
    }
    private void Awake()
    {
        Debug.Log(GameMaster.Phase.Attack.ToString());
    }
}
