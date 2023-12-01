using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropPlace : MonoBehaviour, IDropHandler
{
    [SerializeField] private bool isSelected = false; // ��o�悩
    [SerializeField] private Transform selectedPosition;
    [SerializeField] private Transform supportPosition;
    [SerializeField] private Battler player;
    CardMovement selectingCard;

    //�J�[�h���h���b�O������
    public void OnDrop(PointerEventData eventData)
    {
        if (selectingCard != null && !player.IsSubmit) CardToHand();
        //�I�����ꂽ�J�[�h
        selectingCard = eventData.pointerDrag.GetComponent<CardMovement>();
        //Debug.Log(selectedCard + "ga sentaku");

        //�I�����ꂽ�J�[�h�����݂���ꍇ
        if (selectingCard != null)
        {
            if (isSelected && !player.IsSubmit) CardToField();
            else if (isSelected && player.IsSubmit) CardToSupportField();
            else CardToHand();
        }
    }

    // �J�[�h��I����Ԃɂ���
    private void CardToField()
    {
        selectingCard.MoveToSelect(selectedPosition, isSelected).Forget();
        selectingCard.IsHand = false;
        player.SelectedCard(selectingCard.MyCard);
        //Debug.Log("�J�[�h���t�B�[���h�ɏo���I");
    }
    private void CardToSupportField()
    {
        selectingCard.MoveToSelect(supportPosition, isSelected).Forget();
        selectingCard.IsHand = false;
        player.SelectedCard(selectingCard.MyCard);
    }
    // �J�[�h����D�ɖ߂�
    private void CardToHand()
    {
        selectingCard.MoveToSelect(selectingCard.defaultParent, false).Forget();
        selectingCard.IsHand = true;
        selectingCard = null;
        player.SelectedCard(null);
        //Debug.Log("�J�[�h����D�ɖ߂����I");
    }

    public void ClickOther()
    {
        if (selectingCard == null) return;
        //CardToHand();
    }
}