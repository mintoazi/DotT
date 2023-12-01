using UnityEngine;

public class SelectCard : MonoBehaviour
{
    // 選択されたカードをセットする
    Card selectedCard;
    public Card SelectedCard { get => selectedCard; }

    public void Set(Card card)
    {
        selectedCard = card;
    }

    public void DeleteCard()
    {
        if (selectedCard == null) return;
        selectedCard.Delete();
        selectedCard = null;
    }
}
