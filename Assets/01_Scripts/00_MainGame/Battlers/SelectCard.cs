using UnityEngine;

public class SelectCard : MonoBehaviour
{
    // �I�����ꂽ�J�[�h���Z�b�g����
    Card selectedCard;
    public Card SelectedCard { get => selectedCard; }

    // �I�������J�[�h�̍��W���Z�b�g����
    Transform selectedPosition;
    public Transform SelectedPosition { set => selectedPosition = value; }

    // �I�������J�[�h���q�v�f�ɂ���
    [SerializeField] Transform selectedCardParent;
    

    // �����̎q�v�f�ɂ���E�ʒu�̕ύX
    public void Set(Card card)
    {
        selectedCard = card;
        card.transform.SetParent(selectedCardParent);
        card.transform.position = selectedPosition.position;
    }

    public void DeleteCard()
    {
        Destroy(selectedCard.gameObject);
        selectedCard = null;
    }
}
