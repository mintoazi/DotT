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
        Debug.Log(selectedCard.Base.Name + "が墓地に送られました");
        DestroyImmediate(selectedCard.gameObject);
        selectedCard = null;
    }
}
