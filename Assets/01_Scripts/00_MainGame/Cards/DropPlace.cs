using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropPlace : MonoBehaviour, IDropHandler
{
    [SerializeField] private bool isSelected = false; // ��o�悩
    [SerializeField] private Transform selectedPosition;
    [SerializeField] private Battler player;
    CardMovement selectedCard;

    //�J�[�h���h���b�O������
    public void OnDrop(PointerEventData eventData)
    {
        if (selectedCard != null) CardToHand();
        //�I�����ꂽ�J�[�h
        selectedCard = eventData.pointerDrag.GetComponent<CardMovement>();
        Debug.Log(selectedCard + "ga sentaku");

        //�I�����ꂽ�J�[�h�����݂���ꍇ
        if (selectedCard != null)
        {
            if (isSelected) CardToField(); 
            else CardToHand();
        }
    }

    // �J�[�h��I����Ԃɂ���
    private void CardToField()
    {
        selectedCard.MoveToSelect(selectedPosition, isSelected).Forget();
        selectedCard.IsHand = false;
        player.SelectedCard(selectedCard.MyCard);
        //Debug.Log("�J�[�h���t�B�[���h�ɏo���I");
    }
    // �J�[�h����D�ɖ߂�
    private void CardToHand()
    {
        selectedCard.MoveToSelect(selectedCard.defaultParent, false).Forget();
        selectedCard.IsHand = true;
        selectedCard = null;
        player.SelectedCard(null);
        //Debug.Log("�J�[�h����D�ɖ߂����I");
    }

    public void ClickOther()
    {
        if (selectedCard == null) return;
        //CardToHand();
    }
}