using Cysharp.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropPlace : MonoBehaviour, IDropHandler
{
    [SerializeField] private bool isSelected = false;
    [SerializeField] private Transform selectedPosition;
    public void OnDrop(PointerEventData eventData)
    {
        CardMovement card = eventData.pointerDrag.GetComponent<CardMovement>();
        if (card != null)
        {
            card.defaultParent = this.transform;

            if (isSelected)
            {
                card.MoveToSelect(selectedPosition, isSelected).Forget();
                card.IsHand = false;
            }
            else
            {
                card.MoveToSelect(card.defaultParent, isSelected).Forget();
                card.IsHand = true;
            }
        }
    }
}