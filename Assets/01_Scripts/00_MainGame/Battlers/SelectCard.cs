using UnityEngine;

public class SelectCard : MonoBehaviour
{
    // �I�����ꂽ�J�[�h���Z�b�g����
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
