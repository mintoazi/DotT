using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropPlace : MonoBehaviour, IDropHandler
{
    [SerializeField] private bool isSelected = false;
    [SerializeField] private Transform selectedPosition;
    CardMovement lastCard;
    public void OnDrop(PointerEventData eventData)
    {
        lastCard = eventData.pointerDrag.GetComponent<CardMovement>();
        if (lastCard != null)
        {
            //card.defaultParent = this.transform;

            if (isSelected)
            {
                lastCard.MoveToSelect(selectedPosition, isSelected).Forget();
                lastCard.IsHand = false;
            }
            else
            {
                lastCard.MoveToSelect(lastCard.defaultParent, isSelected).Forget();
                lastCard.IsHand = true;
            }
        }
        lastCard = null;
    }

    public void ClickOther()
    {
        if (lastCard == null) return;
        lastCard.MoveToSelect(lastCard.defaultParent, isSelected).Forget();
        lastCard.IsHand = true;
    }
}