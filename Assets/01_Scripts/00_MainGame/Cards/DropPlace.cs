using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropPlace : MonoBehaviour, IDropHandler
{
    [SerializeField] private bool isSelected = false; // 提出先か
    [SerializeField] private Transform selectedPosition;
    [SerializeField] private Transform supportPosition;
    [SerializeField] private Battler player;
    CardMovement selectingCard;

    //カードをドラッグした時
    public void OnDrop(PointerEventData eventData)
    {
        if (selectingCard != null && !player.IsSubmit) CardToHand();
        //選択されたカード
        selectingCard = eventData.pointerDrag.GetComponent<CardMovement>();
        //Debug.Log(selectedCard + "ga sentaku");

        //選択されたカードが存在する場合
        if (selectingCard != null)
        {
            if (isSelected && !player.IsSubmit) CardToField();
            else if (isSelected && player.IsSubmit) CardToSupportField();
            else CardToHand();
        }
    }

    // カードを選択状態にする
    private void CardToField()
    {
        selectingCard.MoveToSelect(selectedPosition, isSelected).Forget();
        selectingCard.IsHand = false;
        player.SelectedCard(selectingCard.MyCard);
        //Debug.Log("カードがフィールドに出た！");
    }
    private void CardToSupportField()
    {
        selectingCard.MoveToSelect(supportPosition, isSelected).Forget();
        selectingCard.IsHand = false;
        player.SelectedCard(selectingCard.MyCard);
    }
    // カードを手札に戻す
    private void CardToHand()
    {
        selectingCard.MoveToSelect(selectingCard.defaultParent, false).Forget();
        selectingCard.IsHand = true;
        selectingCard = null;
        player.SelectedCard(null);
        //Debug.Log("カードが手札に戻った！");
    }

    public void ClickOther()
    {
        if (selectingCard == null) return;
        //CardToHand();
    }
}