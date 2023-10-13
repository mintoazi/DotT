using UnityEngine;

public class SelectCard : MonoBehaviour
{
    // 選択されたカードをセットする
    Card selectedCard;
    public Card SelectedCard { get => selectedCard; }

    // 選択したカードの座標をセットする
    Transform selectedPosition;
    public Transform SelectedPosition { set => selectedPosition = value; }

    // 選択したカードを子要素にする
    [SerializeField] Transform selectedCardParent;
    

    // 自分の子要素にする・位置の変更
    public void Set(Card card)
    {
        if (selectedPosition == null) return;
        selectedCard = card;
        card.transform.SetParent(selectedCardParent);
        card.transform.position = selectedPosition.position;
        card.SetScale(true);
    }

    public void DeleteCard()
    {
        Destroy(selectedCard.gameObject);
        selectedCard = null;
    }
}
