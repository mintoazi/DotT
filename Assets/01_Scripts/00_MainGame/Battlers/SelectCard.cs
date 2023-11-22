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
        Debug.Log(selectedCard.Base.Name + "����n�ɑ����܂���");
        DestroyImmediate(selectedCard.gameObject);
        selectedCard = null;
    }
}
