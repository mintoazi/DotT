using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropPlace : MonoBehaviour, IDropHandler
{
    [SerializeField] private bool isSelected = false; // 提出先か
    [SerializeField] private Transform selectedPosition;
    [SerializeField] private Battler player;
    CardMovement selectedCard;

    //カードをドラッグした時
    public void OnDrop(PointerEventData eventData)
    {
        if (selectedCard != null) CardToHand();
        //選択されたカード
        selectedCard = eventData.pointerDrag.GetComponent<CardMovement>();
        Debug.Log(selectedCard + "ga sentaku");

        //選択されたカードが存在する場合
        if (selectedCard != null)
        {
            if (isSelected) CardToField(); 
            else CardToHand();
        }
    }

    // カードを選択状態にする
    private void CardToField()
    {
        selectedCard.MoveToSelect(selectedPosition, isSelected).Forget();
        selectedCard.IsHand = false;
        player.SelectedCard(selectedCard.MyCard);
        //Debug.Log("カードがフィールドに出た！");
    }
    // カードを手札に戻す
    private void CardToHand()
    {
        selectedCard.MoveToSelect(selectedCard.defaultParent, false).Forget();
        selectedCard.IsHand = true;
        selectedCard = null;
        player.SelectedCard(null);
        //Debug.Log("カードが手札に戻った！");
    }

    public void ClickOther()
    {
        if (selectedCard == null) return;
        //CardToHand();
    }
}