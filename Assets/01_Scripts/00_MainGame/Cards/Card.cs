using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.ComponentModel;

public class Card : MonoBehaviour
{
    [SerializeField] private GameObject parentObject;

    [SerializeField] private Text cardName;
    [SerializeField] private Text description;
    [SerializeField] private Text cost;
    [SerializeField] private Image cardFrame;
    [SerializeField] private Sprite[] cardSprite = null;
    [SerializeField] private Canvas canvas = null;

    public CardBase Base { get; private set; }
    // �A�N�V������o�^
    public UnityAction<Card> OnClickCard;
    
    public void Set(CardBase cardBase)
    {
        Base = cardBase;
        cardName.text = cardBase.Name;
        description.text = cardBase.Description;
        cost.text = cardBase.Cost.ToString();
        cardFrame.sprite = cardSprite[(int)cardBase.Type];
        
    }

    public void SetLayer(int i)
    {
        canvas.sortingOrder = i;
    }

    public void OnClick()
    {
        //Debug.Log("�J�[�h���I������܂����B");
        OnClickCard?.Invoke(this);
    }

    /*
    -���܂ɕςȂƂ���ɃJ�[�h���u�����o�O�C��
    -�J�[�h�v���C����
    -�ړ�����

    �J�[�h�ƔՖʂ̃��C���[���

    �U������
    �J�[�h���ʎ���
    
    �SPhase��Delay�ǉ�

    �L�����I������

    Photon�Ή�
    */
}
